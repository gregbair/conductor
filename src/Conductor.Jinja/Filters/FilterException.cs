using Conductor.Jinja.Common;

namespace Conductor.Jinja.Filters;

/// <summary>
///     Exception thrown when a filter operation fails.
/// </summary>
public sealed class FilterException : JinjaException
{
    public FilterException(string message) : base(message)
    {
    }

    public FilterException(string message, Position position) : base(message, position)
    {
    }

    public FilterException(string message, Exception innerException) : base(message, innerException)
    {
    }
}