using System.Text;

namespace FulcrumLabs.Conductor.Jinja.Filters.Ansible;

/// <summary>
///     Encodes a string to Base64.
/// </summary>
public sealed class B64EncodeFilter : IFilter
{
    /// <inheritdoc />
    public string Name => "b64encode";

    /// <inheritdoc />
    public object Apply(object? value, object?[] arguments, FilterContext context)
    {
        if (value == null)
        {
            return string.Empty;
        }

        string str = value.ToString() ?? string.Empty;
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        return Convert.ToBase64String(bytes);
    }
}