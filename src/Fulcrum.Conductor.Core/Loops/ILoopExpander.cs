using Fulcrum.Conductor.Core.Tasks;
using Fulcrum.Conductor.Jinja.Rendering;

namespace Fulcrum.Conductor.Core.Loops;

/// <summary>
/// Expands loop definitions into concrete items for iteration.
/// </summary>
public interface ILoopExpander
{
    /// <summary>
    /// Expands a loop definition into an enumerable of items.
    /// </summary>
    IEnumerable<object?> ExpandLoop(LoopDefinition loopDef, TemplateContext context);
}
