namespace FulcrumLabs.Conductor.Jinja.Parsing.Nodes.Expressions;

/// <summary>
///     Represents a unary operation (e.g., not, -).
/// </summary>
public sealed class UnaryExpression : IExpression
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UnaryExpression"/> class.
    /// </summary>
    /// <param name="operatorType">The unary operator.</param>
    /// <param name="operand">The operand expression.</param>
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
    /// <summary>Logical NOT (not).</summary>
    Not,
    /// <summary>Numeric negation (-).</summary>
    Negate
}