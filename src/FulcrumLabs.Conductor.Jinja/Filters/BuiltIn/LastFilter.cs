using System.Collections;

namespace FulcrumLabs.Conductor.Jinja.Filters.BuiltIn;

/// <summary>
///     Returns the last element of a collection.
/// </summary>
public sealed class LastFilter : IFilter
{
    /// <inheritdoc />
    public string Name => "last";

    /// <inheritdoc />
    public object? Apply(object? value, object?[] arguments, FilterContext context)
    {
        if (value == null)
        {
            return null;
        }

        if (value is IList list && list.Count > 0)
        {
            return list[list.Count - 1];
        }

        if (value is IEnumerable enumerable)
        {
            object? last = null;
            foreach (object? item in enumerable)
            {
                last = item;
            }

            return last;
        }

        return null;
    }
}