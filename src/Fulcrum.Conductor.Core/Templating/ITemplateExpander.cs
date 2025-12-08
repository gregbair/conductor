using Fulcrum.Conductor.Jinja.Rendering;

namespace Fulcrum.Conductor.Core.Templating;

/// <summary>
/// Expands Jinja2 templates in task parameters.
/// </summary>
public interface ITemplateExpander
{
    /// <summary>
    /// Expands templates in a dictionary of parameters.
    /// Recursively processes nested dictionaries and lists.
    /// </summary>
    Dictionary<string, object?> ExpandParameters(Dictionary<string, object?> parameters, TemplateContext context);

    /// <summary>
    /// Expands a single template string.
    /// </summary>
    string ExpandString(string template, TemplateContext context);

    /// <summary>
    /// Evaluates a Jinja2 expression and returns the result.
    /// Used for conditionals and loop definitions.
    /// </summary>
    object? EvaluateExpression(string expression, TemplateContext context);
}
