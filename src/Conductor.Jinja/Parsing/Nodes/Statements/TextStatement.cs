namespace Conductor.Jinja.Parsing.Nodes.Statements;

/// <summary>
///     Represents literal text in the template.
/// </summary>
public sealed class TextStatement : IStatement
{
    public TextStatement(string text)
    {
        Text = text;
    }

    /// <summary>
    ///     Gets the literal text content.
    /// </summary>
    public string Text { get; init; }
}