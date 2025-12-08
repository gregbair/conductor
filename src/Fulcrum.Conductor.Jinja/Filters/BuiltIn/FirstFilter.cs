using System.Collections;

namespace Fulcrum.Conductor.Jinja.Filters.BuiltIn;

/// <summary>
///     Returns the first element of a collection.
/// </summary>
public sealed class FirstFilter : IFilter
{
    public string Name => "first";

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