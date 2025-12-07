using Fulcrum.Conductor.Jinja.Filters;
using Fulcrum.Conductor.Jinja.Lexing;
using Fulcrum.Conductor.Jinja.Parsing;
using Fulcrum.Conductor.Jinja.Parsing.Nodes;

namespace Fulcrum.Conductor.Jinja.Rendering;

/// <summary>
///     Represents a compiled Jinja2 template.
/// </summary>
public sealed class Template
{
    private readonly FilterRegistry _filterRegistry;
    private readonly IList<IStatement> _statements;

    private Template(IList<IStatement> statements, FilterRegistry? filterRegistry = null)
    {
        _statements = statements;
        _filterRegistry = filterRegistry ?? FilterRegistry.CreateDefault();
    }

    /// <summary>
    ///     Parses a template string.
    /// </summary>
    public static Template Parse(string templateText, FilterRegistry? filterRegistry = null)
    {
        Lexer lexer = new();
        IList<Token> tokens = lexer.Tokenize(templateText);

        Parser parser = new();
        IList<IStatement> statements = parser.Parse(tokens);

        return new Template(statements, filterRegistry);
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
        Renderer renderer = new(_filterRegistry);
        return renderer.Render(_statements, context);
    }
}