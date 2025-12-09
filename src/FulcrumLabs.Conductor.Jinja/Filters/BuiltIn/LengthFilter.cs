using System.Collections;

namespace FulcrumLabs.Conductor.Jinja.Filters.BuiltIn;

/// <summary>
///     Returns the length of a string or collection.
/// </summary>
public sealed class LengthFilter : IFilter
{
    /// <inheritdoc />
    public string Name => "length";

    /// <inheritdoc />
    public object Apply(object? value, object?[] arguments, FilterContext context)
    {
        switch (value)
        {
            case null:
                return 0;
            case string str:
                return str.Length;
            case ICollection collection:
                return collection.Count;
            case IEnumerable enumerable:
                {
                    int count = 0;
                    foreach (object? _ in enumerable)
                    {
                        count++;
                    }

                    return count;
                }
            default:
                return 0;
        }
    }
}