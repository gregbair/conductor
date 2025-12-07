namespace Fulcrum.Conductor.Jinja.Parsing.Nodes.Expressions;

/// <summary>
///     Represents a binary operation (e.g., +, -, ==, !=, <, >, and, or).
/// </summary>
public sealed class BinaryExpression : IExpression
{
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