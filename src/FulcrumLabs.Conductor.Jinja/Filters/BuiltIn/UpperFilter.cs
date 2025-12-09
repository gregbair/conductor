namespace FulcrumLabs.Conductor.Jinja.Filters.BuiltIn;

/// <summary>
///     Converts a string to uppercase.
/// </summary>
public sealed class UpperFilter : IFilter
{
    /// <inheritdoc />
    public string Name => "upper";

    /// <inheritdoc />
    public object? Apply(object? value, object?[] arguments, FilterContext context)
    {
        if (value == null)
        {
            return null;
        }

        return value.ToString()?.ToUpperInvariant();
    }
}