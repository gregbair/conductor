using System.Text;

using Conductor.Jinja.Parsing.Nodes;
using Conductor.Jinja.Parsing.Nodes.Expressions;
using Conductor.Jinja.Parsing.Nodes.Statements;

namespace Conductor.Jinja.Rendering;

/// <summary>
///     Renders a Jinja2 AST to produce output.
/// </summary>
public sealed class Renderer
{
    private TemplateContext _context = null!;

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
        else
        {
            throw new RenderException($"Unknown statement type: {statement.GetType().Name}");
        }
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

        throw new RenderException($"Unknown expression type: {expression.GetType().Name}");
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