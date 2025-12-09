using System.Text.Json;

namespace FulcrumLabs.Conductor.Jinja.Filters.Ansible;

/// <summary>
///     Parses a JSON string.
/// </summary>
public sealed class FromJsonFilter : IFilter
{
    /// <inheritdoc />
    public string Name => "from_json";

    /// <inheritdoc />
    public object? Apply(object? value, object?[] arguments, FilterContext context)
    {
        if (value == null)
        {
            return null;
        }

        string json = value.ToString() ?? string.Empty;

        try
        {
            return JsonSerializer.Deserialize<object>(json);
        }
        catch (JsonException ex)
        {
            throw new FilterException($"Invalid JSON: {ex.Message}", ex);
        }
    }
}