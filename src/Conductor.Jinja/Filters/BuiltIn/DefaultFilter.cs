namespace Conductor.Jinja.Filters.BuiltIn;

/// <summary>
///     Returns a default value if the input is undefined or falsy.
/// </summary>
public sealed class DefaultFilter : IFilter
{
    public string Name => "default";

    public object? Apply(object? value, object?[] arguments, FilterContext context)
    {
        if (arguments.Length == 0)
        {
            throw new FilterException("default filter requires at least one argument");
        }

        object? defaultValue = arguments[0];
        bool useDefaultOnFalsy = false;

        if (arguments.Length > 1 && arguments[1] is bool boolArg)
        {
            useDefaultOnFalsy = boolArg;
        }

        if (value == null)
        {
            return defaultValue;
        }

        if (useDefaultOnFalsy)
        {
            if (value is bool boolValue && !boolValue)
            {
                return defaultValue;
            }

            if (value is string strValue && string.IsNullOrEmpty(strValue))
            {
                return defaultValue;
            }

            if (value is int intValue && intValue == 0)
            {
                return defaultValue;
            }

            if (value is double doubleValue && doubleValue == 0.0)
            {
                return defaultValue;
            }
        }

        return value;
    }
}