namespace FulcrumLabs.Conductor.Jinja.Parsing.Nodes.Expressions;

/// <summary>
///     Represents a literal value (string, number, boolean, null).
/// </summary>
public sealed class LiteralExpression : IExpression
{
    public LiteralExpression(object? value)
    {
        Value = value;
    }

    /// <summary>
    ///     Gets the literal value.
    /// </summary>
    public object? Value { get; init; }
}