using System.Text.Json;

namespace FulcrumLabs.Conductor.Jinja.Filters.Ansible;

/// <summary>
///     Converts a value to JSON format.
/// </summary>
public sealed class ToJsonFilter : IFilter
{
    /// <inheritdoc />
    public string Name => "to_json";

    /// <inheritdoc />
    public object Apply(object? value, object?[] arguments, FilterContext context)
    {
        if (value == null)
        {
            return "null";
        }

        JsonSerializerOptions options = new() { WriteIndented = false };

        return JsonSerializer.Serialize(value, options);
    }
}