namespace FulcrumLabs.Conductor.Jinja.Filters.BuiltIn;

/// <summary>
///     Splits a string into a list.
/// </summary>
public sealed class SplitFilter : IFilter
{
    /// <inheritdoc />
    public string Name => "split";

    /// <inheritdoc />
    public object Apply(object? value, object?[] arguments, FilterContext context)
    {
        if (value == null)
        {
            return new string[0];
        }

        string str = value.ToString() ?? string.Empty;
        string separator = arguments.Length > 0 ? arguments[0]?.ToString() ?? " " : " ";

        return str.Split(separator);
    }
}