using System.Collections;
using System.Reflection;
using System.Text;

using FulcrumLabs.Conductor.Jinja.Filters;
using FulcrumLabs.Conductor.Jinja.Parsing.Nodes;
using FulcrumLabs.Conductor.Jinja.Parsing.Nodes.Expressions;
using FulcrumLabs.Conductor.Jinja.Parsing.Nodes.Statements;

namespace FulcrumLabs.Conductor.Jinja.Rendering;

/// <summary>
///     Renders a Jinja2 AST to produce output.
/// </summary>
public sealed class Renderer
{
    private readonly FilterRegistry _filterRegistry;
    private TemplateContext _context = null!;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Renderer"/> class.
    /// </summary>
    /// <param name="filterRegistry">The filter registry to use for filter lookups. If null, the default registry is used.</param>
    public Renderer(FilterRegistry? filterRegistry = null)
    {
        _filterRegistry = filterRegistry ?? FilterRegistry.CreateDefault();
    }

    /// <summary>
    ///     Renders an AST with the given context.
    /// </summary>
    public string Render(IList<IStatement> statements, TemplateContext context)
    {
        _context = context;
        StringBuilder output = new();

        foreach (IStatement statement in statements)
        {
            VisitStatement(statement, output);
        }

        return output.ToString();
    }

    private void VisitStatement(IStatement statement, StringBuilder output)
    {
        if (statement is TextStatement textStatement)
        {
            output.Append(textStatement.Text);
        }
        else if (statement is OutputStatement outputStatement)
        {
            object? value = EvaluateExpression(outputStatement.Expression);
            output.Append(FormatValue(value));
        }
        else if (statement is ForStatement forStatement)
        {
            ExecuteForLoop(forStatement, output);
        }
        else if (statement is IfStatement ifStatement)
        {
            ExecuteIfStatement(ifStatement, output);
        }
        else
        {
            throw new RenderException($"Unknown statement type: {statement.GetType().Name}");
        }
    }

    private void ExecuteForLoop(ForStatement forStatement, StringBuilder output)
    {
        object? iterableValue = EvaluateExpression(forStatement.Iterable);

        if (iterableValue == null)
        {
            return;
        }

        if (iterableValue is not IEnumerable enumerable)
        {
            throw new RenderException($"Cannot iterate over non-iterable type: {iterableValue.GetType().Name}");
        }

        TemplateContext loopContext = _context.CreateChildScope();
        TemplateContext previousContext = _context;

        try
        {
            _context = loopContext;

            foreach (object? item in enumerable)
            {
                _context.SetVariable(forStatement.LoopVariable, item);

                foreach (IStatement statement in forStatement.Body)
                {
                    VisitStatement(statement, output);
                }
            }
        }
        finally
        {
            _context = previousContext;
        }
    }

    private void ExecuteIfStatement(IfStatement ifStatement, StringBuilder output)
    {
        if (IsTruthy(EvaluateExpression(ifStatement.Condition)))
        {
            foreach (IStatement statement in ifStatement.ThenBody)
            {
                VisitStatement(statement, output);
            }

            return;
        }

        foreach ((IExpression condition, IList<IStatement> body) in ifStatement.ElifBranches)
        {
            if (IsTruthy(EvaluateExpression(condition)))
            {
                foreach (IStatement statement in body)
                {
                    VisitStatement(statement, output);
                }

                return;
            }
        }

        if (ifStatement.ElseBody != null)
        {
            foreach (IStatement statement in ifStatement.ElseBody)
            {
                VisitStatement(statement, output);
            }
        }
    }

    private bool IsTruthy(object? value)
    {
        if (value == null)
        {
            return false;
        }

        if (value is bool boolValue)
        {
            return boolValue;
        }

        if (value is string stringValue)
        {
            return !string.IsNullOrEmpty(stringValue);
        }

        if (value is int intValue)
        {
            return intValue != 0;
        }

        if (value is double doubleValue)
        {
            return doubleValue != 0.0;
        }

        if (value is ICollection collection)
        {
            return collection.Count > 0;
        }

        return true;
    }

    private object? EvaluateExpression(IExpression expression)
    {
        if (expression is LiteralExpression literalExpression)
        {
            return literalExpression.Value;
        }

        if (expression is VariableExpression variableExpression)
        {
            return _context.GetVariable(variableExpression.Name);
        }

        if (expression is BinaryExpression binaryExpression)
        {
            return EvaluateBinaryExpression(binaryExpression);
        }

        if (expression is UnaryExpression unaryExpression)
        {
            return EvaluateUnaryExpression(unaryExpression);
        }

        if (expression is MemberExpression memberExpression)
        {
            return EvaluateMemberExpression(memberExpression);
        }

        if (expression is IndexExpression indexExpression)
        {
            return EvaluateIndexExpression(indexExpression);
        }

        if (expression is FilterExpression filterExpression)
        {
            return EvaluateFilterExpression(filterExpression);
        }

        throw new RenderException($"Unknown expression type: {expression.GetType().Name}");
    }

    private object? EvaluateBinaryExpression(BinaryExpression expression)
    {
        object? left = EvaluateExpression(expression.Left);
        object? right = EvaluateExpression(expression.Right);

        switch (expression.Operator)
        {
            case BinaryOperator.Add:
                return Convert.ToDouble(left) + Convert.ToDouble(right);
            case BinaryOperator.Subtract:
                return Convert.ToDouble(left) - Convert.ToDouble(right);
            case BinaryOperator.Multiply:
                return Convert.ToDouble(left) * Convert.ToDouble(right);
            case BinaryOperator.Divide:
                return Convert.ToDouble(left) / Convert.ToDouble(right);
            case BinaryOperator.FloorDivide:
                return Math.Floor(Convert.ToDouble(left) / Convert.ToDouble(right));
            case BinaryOperator.Modulo:
                return Convert.ToDouble(left) % Convert.ToDouble(right);
            case BinaryOperator.Power:
                return Math.Pow(Convert.ToDouble(left), Convert.ToDouble(right));

            case BinaryOperator.Equal:
                return Equals(left, right);
            case BinaryOperator.NotEqual:
                return !Equals(left, right);
            case BinaryOperator.LessThan:
                return Convert.ToDouble(left) < Convert.ToDouble(right);
            case BinaryOperator.LessThanOrEqual:
                return Convert.ToDouble(left) <= Convert.ToDouble(right);
            case BinaryOperator.GreaterThan:
                return Convert.ToDouble(left) > Convert.ToDouble(right);
            case BinaryOperator.GreaterThanOrEqual:
                return Convert.ToDouble(left) >= Convert.ToDouble(right);

            case BinaryOperator.And:
                return IsTruthy(left) && IsTruthy(right);
            case BinaryOperator.Or:
                return IsTruthy(left) || IsTruthy(right);

            case BinaryOperator.In:
                if (right is IEnumerable enumerable)
                {
                    foreach (object? item in enumerable)
                    {
                        if (Equals(left, item))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                throw new RenderException(
                    $"'in' operator requires iterable on right side, got {right?.GetType().Name}");

            default:
                throw new RenderException($"Unknown binary operator: {expression.Operator}");
        }
    }

    private object? EvaluateUnaryExpression(UnaryExpression expression)
    {
        object? operand = EvaluateExpression(expression.Operand);

        switch (expression.Operator)
        {
            case UnaryOperator.Not:
                return !IsTruthy(operand);
            case UnaryOperator.Negate:
                return -Convert.ToDouble(operand);
            default:
                throw new RenderException($"Unknown unary operator: {expression.Operator}");
        }
    }

    private object? EvaluateMemberExpression(MemberExpression expression)
    {
        object? obj = EvaluateExpression(expression.Object);

        if (obj == null)
        {
            return null;
        }

        Type type = obj.GetType();
        PropertyInfo? property = type.GetProperty(expression.PropertyName);

        if (property != null)
        {
            return property.GetValue(obj);
        }

        FieldInfo? field = type.GetField(expression.PropertyName);

        if (field != null)
        {
            return field.GetValue(obj);
        }

        return null;
    }

    private object? EvaluateIndexExpression(IndexExpression expression)
    {
        object? obj = EvaluateExpression(expression.Object);
        object? index = EvaluateExpression(expression.Index);

        if (obj == null || index == null)
        {
            return null;
        }

        if (obj is IDictionary dictionary)
        {
            return dictionary[index];
        }

        if (obj is IList list)
        {
            int intIndex = Convert.ToInt32(index);
            if (intIndex < 0 || intIndex >= list.Count)
            {
                return null;
            }

            return list[intIndex];
        }

        if (obj is string str)
        {
            int intIndex = Convert.ToInt32(index);
            if (intIndex < 0 || intIndex >= str.Length)
            {
                return null;
            }

            return str[intIndex].ToString();
        }

        Type type = obj.GetType();
        PropertyInfo? indexer = type.GetProperty("Item");

        if (indexer != null)
        {
            return indexer.GetValue(obj, new[] { index });
        }

        throw new RenderException($"Cannot index type: {type.Name}");
    }

    private object? EvaluateFilterExpression(FilterExpression expression)
    {
        object? value = EvaluateExpression(expression.Value);

        IFilter? filter = _filterRegistry.GetFilter(expression.FilterName);

        if (filter == null)
        {
            throw new RenderException($"Unknown filter: {expression.FilterName}");
        }

        object?[] arguments = expression.Arguments
            .Select(arg => EvaluateExpression(arg))
            .ToArray();

        FilterContext filterContext = new(_context);

        try
        {
            return filter.Apply(value, arguments, filterContext);
        }
        catch (Exception ex) when (ex is not FilterException)
        {
            throw new RenderException($"Filter '{expression.FilterName}' failed: {ex.Message}", ex);
        }
    }

    private string FormatValue(object? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (value is bool boolValue)
        {
            return boolValue ? "True" : "False";
        }

        return value.ToString() ?? string.Empty;
    }
}