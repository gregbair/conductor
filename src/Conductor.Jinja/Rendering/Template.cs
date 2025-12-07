using Conductor.Jinja.Lexing;
using Conductor.Jinja.Parsing;
using Conductor.Jinja.Parsing.Nodes;

namespace Conductor.Jinja.Rendering;

/// <summary>
///     Represents a compiled Jinja2 template.
/// </summary>
public sealed class Template
{
    private readonly IList<IStatement> _statements;

    private Template(IList<IStatement> statements)
    {
        _statements = statements;
    }

    /// <summary>
    ///     Parses a template string.
    /// </summary>
    public static Template Parse(string templateText)
    {
        Lexer lexer = new();
        IList<Token> tokens = lexer.Tokenize(templateText);

        Parser parser = new();
        IList<IStatement> statements = parser.Parse(tokens);

        return new Template(statements);
    }

    /// <summary>
    ///     Renders the template with the given variables.
    /// </summary>
    public string Render(object? variables = null)
    {
        TemplateContext context = TemplateContext.FromObject(variables);
        return Render(context);
    }

    /// <summary>
    ///     Renders the template with the given context.
    /// </summary>
    public string Render(TemplateContext context)
    {
        Renderer renderer = new();
        return renderer.Render(_statements, context);
    }
}