using System.Collections;

using FulcrumLabs.Conductor.Core.Templating;
using FulcrumLabs.Conductor.Jinja.Rendering;

namespace FulcrumLabs.Conductor.Core.Conditionals;

/// <summary>
/// Evaluates conditionals using Jinja2 expression syntax.
/// </summary>
public sealed class ConditionalEvaluator : IConditionalEvaluator
{
    private readonly ITemplateExpander _templateExpander;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalEvaluator"/> class.
    /// </summary>
    /// <param name="templateExpander">The template expander to use for evaluating expressions.</param>
    public ConditionalEvaluator(ITemplateExpander templateExpander)
    {
        _templateExpander = templateExpander;
    }

    /// <inheritdoc />
    public bool Evaluate(string? condition, TemplateContext context)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            return true; // No condition = always true
        }

        object? result = _templateExpander.EvaluateExpression(condition, context);
        return IsTruthy(result);
    }

    private bool IsTruthy(object? value)
    {
        // Use same truthiness logic as Jinja2 Renderer
        if (value == null)
        {
            return false;
        }

        if (value is bool boolValue)
        {
            return boolValue;
        }

        if (value is string stringValue)
        {
            return !string.IsNullOrEmpty(stringValue);
        }

        if (value is int intValue)
        {
            return intValue != 0;
        }

        if (value is double doubleValue)
        {
            return doubleValue != 0.0;
        }

        if (value is ICollection collection)
        {
            return collection.Count > 0;
        }

        return true;
    }
}
