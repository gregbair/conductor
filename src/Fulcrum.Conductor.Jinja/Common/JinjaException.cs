namespace Fulcrum.Conductor.Jinja.Common;

/// <summary>
///     Base exception class for all Jinja2 template engine errors.
/// </summary>
public class JinjaException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="JinjaException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public JinjaException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="JinjaException"/> class with a specified error message and position.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="position">The position in the template where the error occurred.</param>
    public JinjaException(string message, Position position)
        : base($"{message} at {position}")
    {
        Position = position;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="JinjaException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public JinjaException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="JinjaException"/> class with a specified error message, position, and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="position">The position in the template where the error occurred.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public JinjaException(string message, Position position, Exception innerException)
        : base($"{message} at {position}", innerException)
    {
        Position = position;
    }

    /// <summary>
    ///     Gets the position in the template where the error occurred.
    /// </summary>
    public Position? Position { get; }
}