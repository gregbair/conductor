using System.Collections;

using FulcrumLabs.Conductor.Jinja.Rendering;

using FulcrumLabs.Conductor.Core.Templating;

namespace FulcrumLabs.Conductor.Core.Tasks;

/// <summary>
///     Implements the 'with_items:' syntax for iterating over items.
///     Functionally equivalent to SimpleLoopDefinition but represents Ansible's with_items syntax.
/// </summary>
public sealed class WithItemsLoopDefinition : LoopDefinition
{
    /// <summary>
    ///     The items to iterate over (can be a template string, list, or variable reference).
    /// </summary>
    public object? Items { get; init; }

    public override IEnumerable<object?> GetItems(TemplateContext context, ITemplateExpander expander)
    {
        switch (Items)
        {
            case null:
                return Array.Empty<object?>();
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
            null => Array.Empty<object?>(),
            string str => [str],
            IEnumerable enumerable => enumerable.Cast<object?>().ToList(),
            _ => [value]
        };
    }
}