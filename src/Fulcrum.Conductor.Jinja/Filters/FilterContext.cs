using Fulcrum.Conductor.Jinja.Rendering;

namespace Fulcrum.Conductor.Jinja.Filters;

/// <summary>
///     Context passed to filters during rendering.
/// </summary>
public sealed class FilterContext
{
    public FilterContext(TemplateContext templateContext)
    {
        TemplateContext = templateContext;
    }

    /// <summary>
    ///     Gets the template context.
    /// </summary>
    public TemplateContext TemplateContext { get; init; }
}