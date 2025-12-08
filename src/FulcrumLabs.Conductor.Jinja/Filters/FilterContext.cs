using FulcrumLabs.Conductor.Jinja.Rendering;

namespace FulcrumLabs.Conductor.Jinja.Filters;

/// <summary>
///     Context passed to filters during rendering.
/// </summary>
public sealed class FilterContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FilterContext"/> class.
    /// </summary>
    /// <param name="templateContext">The template context containing variables and rendering state.</param>
    public FilterContext(TemplateContext templateContext)
    {
        TemplateContext = templateContext;
    }

    /// <summary>
    ///     Gets the template context.
    /// </summary>
    public TemplateContext TemplateContext { get; init; }
}