using System.Collections;

namespace Fulcrum.Conductor.Jinja.Filters.BuiltIn;

/// <summary>
///     Returns the length of a string or collection.
/// </summary>
public sealed class LengthFilter : IFilter
{
    public string Name => "length";

    public object? Apply(object? value, object?[] arguments, FilterContext context)
    {
        if (value == null)
        {
            return 0;
        }

        if (value is string str)
        {
            return str.Length;
        }

        if (value is ICollection collection)
        {
            return collection.Count;
        }

        if (value is IEnumerable enumerable)
        {
            int count = 0;
            foreach (object? _ in enumerable)
            {
                count++;
            }

            return count;
        }

        return 0;
    }
}