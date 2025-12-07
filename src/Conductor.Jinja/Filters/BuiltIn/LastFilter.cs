using System.Collections;

namespace Conductor.Jinja.Filters.BuiltIn;

/// <summary>
///     Returns the last element of a collection.
/// </summary>
public sealed class LastFilter : IFilter
{
    public string Name => "last";

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