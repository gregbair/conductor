namespace Conductor.Jinja.Filters.BuiltIn;

/// <summary>
///     Converts a string to uppercase.
/// </summary>
public sealed class UpperFilter : IFilter
{
    public string Name => "upper";

    public object? Apply(object? value, object?[] arguments, FilterContext context)
    {
        if (value == null)
        {
            return null;
        }

        return value.ToString()?.ToUpperInvariant();
    }
}