using System.Globalization;

namespace Fulcrum.Conductor.Jinja.Filters.BuiltIn;

/// <summary>
///     Capitalizes the first character of a string.
/// </summary>
public sealed class CapitalizeFilter : IFilter
{
    public string Name => "capitalize";

    public object? Apply(object? value, object?[] arguments, FilterContext context)
    {
        if (value == null)
        {
            return null;
        }

        string str = value.ToString() ?? string.Empty;

        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        return char.ToUpper(str[0], CultureInfo.InvariantCulture) + str.Substring(1).ToLowerInvariant();
    }
}