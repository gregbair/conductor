namespace FulcrumLabs.Conductor.Jinja.Parsing.Nodes.Statements;

/// <summary>
///     Represents literal text in the template.
/// </summary>
public sealed class TextStatement : IStatement
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TextStatement"/> class.
    /// </summary>
    /// <param name="text">The literal text content.</param>
    public TextStatement(string text)
    {
        Text = text;
    }

    /// <summary>
    ///     Gets the literal text content.
    /// </summary>
    public string Text { get; init; }
}