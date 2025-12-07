using System.Text;

namespace Conductor.Jinja.Filters.Ansible;

/// <summary>
///     Decodes a Base64 string.
/// </summary>
public sealed class B64DecodeFilter : IFilter
{
    public string Name => "b64decode";

    public object? Apply(object? value, object?[] arguments, FilterContext context)
    {
        if (value == null)
        {
            return string.Empty;
        }

        string str = value.ToString() ?? string.Empty;

        try
        {
            byte[] bytes = Convert.FromBase64String(str);
            return Encoding.UTF8.GetString(bytes);
        }
        catch (FormatException ex)
        {
            throw new FilterException($"Invalid Base64 string: {ex.Message}", ex);
        }
    }
}