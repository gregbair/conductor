using System.Collections;

using Fulcrum.Conductor.Jinja.Filters;
using Fulcrum.Conductor.Jinja.Rendering;

namespace Fulcrum.Conductor.Core.Templating;

/// <summary>
///     Expands templates using the Conductor Jinja2 template engine.
/// </summary>
public sealed class TemplateExpander(FilterRegistry? filterRegistry = null) : ITemplateExpander
{
    private readonly FilterRegistry _filterRegistry = filterRegistry ?? FilterRegistry.CreateDefault();

    public Dictionary<string, object?> ExpandParameters(Dictionary<string, object?> parameters, TemplateContext context)
    {
        Dictionary<string, object?> expanded = new();

        foreach ((string key, object? value) in parameters)
        {
            expanded[key] = ExpandValue(value, context);
        }

        return expanded;
    }

    public string ExpandString(string template, TemplateContext context)
    {
        if (string.IsNullOrEmpty(template))
        {
            return template;
        }

        // Check if the string contains Jinja2 syntax
        if (!template.Contains("{{") && !template.Contains("{%"))
        {
            return template;
        }

        try
        {
            Template parsedTemplate = Template.Parse(template, _filterRegistry);
            return parsedTemplate.Render(context);
        }
        catch (Exception ex)
        {
            throw new TemplateExpansionException($"Failed to expand template '{template}': {ex.Message}", ex);
        }
    }

    public object? EvaluateExpression(string expression, TemplateContext context)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return null;
        }

        try
        {
            Template template = Template.Parse($"{{{{ {expression} }}}}", _filterRegistry);
            string result = template.Render(context);

            // Try to parse the result back to the original type
            // For simple cases, the result is a string representation
            return ParseRenderedValue(result, expression, context);
        }
        catch (Exception ex)
        {
            throw new TemplateExpansionException($"Failed to evaluate expression '{expression}': {ex.Message}", ex);
        }
    }

    private static object? ParseRenderedValue(string renderedValue, string originalExpression, TemplateContext context)
    {
        // For expressions that are just variable names, get the actual value from context
        if (!originalExpression.Contains(' ') && !originalExpression.Contains('(') && !originalExpression.Contains('['))
        {
            // Might be a simple variable reference
            object? directValue = context.GetVariable(originalExpression);
            if (directValue != null)
            {
                return directValue;
            }
        }

        // Otherwise, try to parse the rendered string value
        if (string.IsNullOrEmpty(renderedValue))
        {
            return null;
        }

        // Try to parse as bool
        if (bool.TryParse(renderedValue, out bool boolValue))
        {
            return boolValue;
        }

        // Try to parse as int
        if (int.TryParse(renderedValue, out int intValue))
        {
            return intValue;
        }

        // Try to parse as double
        if (double.TryParse(renderedValue, out double doubleValue))
        {
            return doubleValue;
        }

        // Return as string
        return renderedValue;
    }

    private object? ExpandValue(object? value, TemplateContext context)
    {
        return value switch
        {
            string str => ExpandString(str, context),
            Dictionary<string, object?> dict => ExpandParameters(dict, context),
            IList list => ExpandList(list, context),
            _ => value
        };
    }

    private IList ExpandList(IList list, TemplateContext context)
    {
        return (from object? item in list select ExpandValue(item, context)).ToList();
    }
}

/// <summary>
///     Exception thrown when template expansion fails.
/// </summary>
public sealed class TemplateExpansionException : Exception
{
    public TemplateExpansionException(string message) : base(message)
    {
    }

    public TemplateExpansionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}