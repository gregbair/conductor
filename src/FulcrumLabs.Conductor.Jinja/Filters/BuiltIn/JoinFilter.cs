using System.Collections;

namespace FulcrumLabs.Conductor.Jinja.Filters.BuiltIn;

/// <summary>
///     Joins a collection into a string with a separator.
/// </summary>
public sealed class JoinFilter : IFilter
{
    /// <inheritdoc />
    public string Name => "join";

    /// <inheritdoc />
    public object? Apply(object? value, object?[] arguments, FilterContext context)
    {
        if (value == null)
        {
            return string.Empty;
        }

        string separator = arguments.Length > 0 ? arguments[0]?.ToString() ?? "" : "";

        if (value is IEnumerable enumerable)
        {
            List<string> items = new();
            foreach (object? item in enumerable)
            {
                items.Add(item?.ToString() ?? string.Empty);
            }

            return string.Join(separator, items);
        }

        return value.ToString();
    }
}