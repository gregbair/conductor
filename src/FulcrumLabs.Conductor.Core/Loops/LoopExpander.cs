using FulcrumLabs.Conductor.Core.Tasks;
using FulcrumLabs.Conductor.Core.Templating;
using FulcrumLabs.Conductor.Jinja.Rendering;

namespace FulcrumLabs.Conductor.Core.Loops;

/// <summary>
/// Expands loops using template expansion and strategy pattern.
/// </summary>
public sealed class LoopExpander : ILoopExpander
{
    private readonly ITemplateExpander _templateExpander;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoopExpander"/> class.
    /// </summary>
    /// <param name="templateExpander">The template expander to use for expanding loop expressions.</param>
    public LoopExpander(ITemplateExpander templateExpander)
    {
        _templateExpander = templateExpander;
    }

    /// <inheritdoc />
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
    /// <summary>
    /// Initializes a new instance of the <see cref="LoopExpansionException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public LoopExpansionException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoopExpansionException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public LoopExpansionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
