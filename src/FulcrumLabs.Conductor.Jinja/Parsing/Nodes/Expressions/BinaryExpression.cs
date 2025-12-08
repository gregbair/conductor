namespace FulcrumLabs.Conductor.Jinja.Parsing.Nodes.Expressions;

/// <summary>
///     Represents a binary operation (e.g., +, -, ==, !=, &lt;, &gt;, and, or).
/// </summary>
public sealed class BinaryExpression : IExpression
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BinaryExpression"/> class.
    /// </summary>
    /// <param name="left">The left operand of the binary operation.</param>
    /// <param name="operatorType">The binary operator.</param>
    /// <param name="right">The right operand of the binary operation.</param>
    public BinaryExpression(IExpression left, BinaryOperator operatorType, IExpression right)
    {
        Left = left;
        Operator = operatorType;
        Right = right;
    }

    /// <summary>
    ///     Gets the left operand.
    /// </summary>
    public IExpression Left { get; init; }

    /// <summary>
    ///     Gets the operator.
    /// </summary>
    public BinaryOperator Operator { get; init; }

    /// <summary>
    ///     Gets the right operand.
    /// </summary>
    public IExpression Right { get; init; }
}

/// <summary>
///     Binary operators supported in Jinja2 expressions.
/// </summary>
public enum BinaryOperator
{
    // Arithmetic
    Add,
    Subtract,
    Multiply,
    Divide,
    FloorDivide,
    Modulo,
    Power,

    // Comparison
    Equal,
    NotEqual,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual,

    // Logical
    And,
    Or,

    // Membership
    In
}