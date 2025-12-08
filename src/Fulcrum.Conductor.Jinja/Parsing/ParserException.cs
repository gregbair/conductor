using Fulcrum.Conductor.Jinja.Common;

namespace Fulcrum.Conductor.Jinja.Parsing;

/// <summary>
///     Exception thrown when an error occurs during parsing.
/// </summary>
public sealed class ParserException : JinjaException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ParserException"/> class with a specified error message and position.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="position">The position in the template where the error occurred.</param>
    public ParserException(string message, Position position)
        : base(message, position)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ParserException"/> class with a specified error message, position, and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="position">The position in the template where the error occurred.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ParserException(string message, Position position, Exception innerException)
        : base(message, position, innerException)
    {
    }
}