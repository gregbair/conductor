namespace Conductor.Jinja.Parsing.Nodes.Expressions;

/// <summary>
///     Represents a unary operation (e.g., not, -).
/// </summary>
public sealed class UnaryExpression : IExpression
{
    public UnaryExpression(UnaryOperator operatorType, IExpression operand)
    {
        Operator = operatorType;
        Operand = operand;
    }

    /// <summary>
    ///     Gets the operator.
    /// </summary>
    public UnaryOperator Operator { get; init; }

    /// <summary>
    ///     Gets the operand.
    /// </summary>
    public IExpression Operand { get; init; }
}

/// <summary>
///     Unary operators supported in Jinja2 expressions.
/// </summary>
public enum UnaryOperator
{
    Not,
    Negate
}