using System.Collections;

using FulcrumLabs.Conductor.Jinja.Rendering;

using FulcrumLabs.Conductor.Core.Templating;

namespace FulcrumLabs.Conductor.Core.Tasks;

/// <summary>
///     Implements the 'loop:' syntax for iterating over items.
/// </summary>
public sealed class SimpleLoopDefinition : LoopDefinition
{
    /// <summary>
    ///     The loop items (can be a template string, list, or variable reference).
    /// </summary>
    public object? Items { get; init; }

    /// <inheritdoc />
    public override IEnumerable<object?> GetItems(TemplateContext context, ITemplateExpander expander)
    {
        switch (Items)
        {
            case null:
                return [];
            // If Items is a string, it might be a template expression
            case string itemsString:
                object? expandedItems = expander.EvaluateExpression(itemsString, context);
                return ConvertToEnumerable(expandedItems);
            default:
                // If Items is already an enumerable, use it directly
                return ConvertToEnumerable(Items);
        }
    }

    private static IEnumerable<object?> ConvertToEnumerable(object? value)
    {
        return value switch
        {
            null => [],
            string str => [str],
            IEnumerable enumerable => enumerable.Cast<object?>().ToList(),
            _ => (IEnumerable<object?>)[value]
        };
    }
}