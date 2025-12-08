using Fulcrum.Conductor.Jinja.Common;

namespace Fulcrum.Conductor.Jinja.Filters;

/// <summary>
///     Exception thrown when a filter operation fails.
/// </summary>
public sealed class FilterException : JinjaException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FilterException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public FilterException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FilterException"/> class with a specified error message and position.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="position">The position in the template where the error occurred.</param>
    public FilterException(string message, Position position) : base(message, position)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FilterException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public FilterException(string message, Exception innerException) : base(message, innerException)
    {
    }
}