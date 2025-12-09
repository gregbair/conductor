namespace FulcrumLabs.Conductor.Jinja.Parsing.Nodes.Expressions;

/// <summary>
///     Represents a literal value (string, number, boolean, null).
/// </summary>
public sealed class LiteralExpression : IExpression
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LiteralExpression"/> class.
    /// </summary>
    /// <param name="value">The literal value.</param>
    public LiteralExpression(object? value)
    {
        Value = value;
    }

    /// <summary>
    ///     Gets the literal value.
    /// </summary>
    public object? Value { get; init; }
}