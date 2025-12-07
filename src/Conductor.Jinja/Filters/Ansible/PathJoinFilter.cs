using System.Collections;

namespace Conductor.Jinja.Filters.Ansible;

/// <summary>
///     Joins path components using Path.Combine.
/// </summary>
public sealed class PathJoinFilter : IFilter
{
    public string Name => "path_join";

    public object? Apply(object? value, object?[] arguments, FilterContext context)
    {
        List<string> parts = new();

        if (value != null)
        {
            if (value is IEnumerable enumerable and not string)
            {
                foreach (object? item in enumerable)
                {
                    if (item != null)
                    {
                        parts.Add(item.ToString() ?? string.Empty);
                    }
                }
            }
            else
            {
                parts.Add(value.ToString() ?? string.Empty);
            }
        }

        foreach (object? arg in arguments)
        {
            if (arg != null)
            {
                parts.Add(arg.ToString() ?? string.Empty);
            }
        }

        if (parts.Count == 0)
        {
            return string.Empty;
        }

        return Path.Combine(parts.ToArray());
    }
}