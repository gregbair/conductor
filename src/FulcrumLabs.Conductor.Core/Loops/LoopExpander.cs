using FulcrumLabs.Conductor.Jinja.Rendering;

using FulcrumLabs.Conductor.Core.Tasks;
using FulcrumLabs.Conductor.Core.Templating;

namespace FulcrumLabs.Conductor.Core.Loops;

/// <summary>
/// Expands loops using template expansion and strategy pattern.
/// </summary>
public sealed class LoopExpander : ILoopExpander
{
    private readonly ITemplateExpander _templateExpander;

    public LoopExpander(ITemplateExpander templateExpander)
    {
        _templateExpander = templateExpander;
    }

    public IEnumerable<object?> ExpandLoop(LoopDefinition loopDef, TemplateContext context)
    {
        try
        {
            // Delegate to the LoopDefinition's GetItems method (Strategy pattern)
            return loopDef.GetItems(context, _templateExpander);
        }
        catch (Exception ex)
        {
            throw new LoopExpansionException($"Failed to expand loop: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// Exception thrown when loop expansion fails.
/// </summary>
public sealed class LoopExpansionException : Exception
{
    public LoopExpansionException(string message) : base(message)
    {
    }

    public LoopExpansionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
