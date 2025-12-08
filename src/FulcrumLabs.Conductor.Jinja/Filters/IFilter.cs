namespace FulcrumLabs.Conductor.Jinja.Filters;

/// <summary>
///     Interface for Jinja2 filters.
/// </summary>
public interface IFilter
{
    /// <summary>
    ///     Gets the name of the filter.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Applies the filter to the given value.
    /// </summary>
    /// <param name="value">The input value to filter.</param>
    /// <param name="arguments">Optional arguments passed to the filter.</param>
    /// <param name="context">The filter context.</param>
    /// <returns>The filtered value.</returns>
    object? Apply(object? value, object?[] arguments, FilterContext context);
}