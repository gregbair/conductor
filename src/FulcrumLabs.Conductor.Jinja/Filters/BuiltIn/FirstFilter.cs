using System.Collections;

namespace FulcrumLabs.Conductor.Jinja.Filters.BuiltIn;

/// <summary>
///     Returns the first element of a collection.
/// </summary>
public sealed class FirstFilter : IFilter
{
    /// <inheritdoc />
    public string Name => "first";

    /// <inheritdoc />
    public object? Apply(object? value, object?[] arguments, FilterContext context)
    {
        if (value == null)
        {
            return null;
        }

        if (value is IEnumerable enumerable)
        {
            foreach (object? item in enumerable)
            {
                return item;
            }
        }

        return null;
    }
}