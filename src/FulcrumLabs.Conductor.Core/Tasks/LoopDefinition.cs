using FulcrumLabs.Conductor.Core.Templating;
using FulcrumLabs.Conductor.Jinja.Rendering;

namespace FulcrumLabs.Conductor.Core.Tasks;

/// <summary>
/// Base class for loop definitions using Strategy pattern.
/// Each loop type (loop, with_items, with_dict, etc.) implements this class.
/// </summary>
public abstract class LoopDefinition
{
    /// <summary>
    /// Gets the items to iterate over.
    /// </summary>
    public abstract IEnumerable<object?> GetItems(TemplateContext context, ITemplateExpander expander);
}
