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
    /// <summary>Addition (+).</summary>
    Add,
    /// <summary>Subtraction (-).</summary>
    Subtract,
    /// <summary>Multiplication (*).</summary>
    Multiply,
    /// <summary>Division (/).</summary>
    Divide,
    /// <summary>Floor division (//).</summary>
    FloorDivide,
    /// <summary>Modulo (%).</summary>
    Modulo,
    /// <summary>Exponentiation (**).</summary>
    Power,

    // Comparison
    /// <summary>Equality (==).</summary>
    Equal,
    /// <summary>Inequality (!=).</summary>
    NotEqual,
    /// <summary>Less than (&lt;).</summary>
    LessThan,
    /// <summary>Less than or equal (&lt;=).</summary>
    LessThanOrEqual,
    /// <summary>Greater than (&gt;).</summary>
    GreaterThan,
    /// <summary>Greater than or equal (&gt;=).</summary>
    GreaterThanOrEqual,

    // Logical
    /// <summary>Logical AND.</summary>
    And,
    /// <summary>Logical OR.</summary>
    Or,

    // Membership
    /// <summary>Membership test (in).</summary>
    In
}