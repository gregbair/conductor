namespace FulcrumLabs.Conductor.Jinja.Filters.BuiltIn;

/// <summary>
///     Converts a string to lowercase.
/// </summary>
public sealed class LowerFilter : IFilter
{
    /// <inheritdoc />
    public string Name => "lower";

    /// <inheritdoc />
    public object? Apply(object? value, object?[] arguments, FilterContext context)
    {
        if (value == null)
        {
            return null;
        }

        return value.ToString()?.ToLowerInvariant();
    }
}