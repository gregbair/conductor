using FulcrumLabs.Conductor.Jinja.Common;

namespace FulcrumLabs.Conductor.Jinja.Lexing;

/// <summary>
///     Exception thrown when an error occurs during lexical analysis.
/// </summary>
public sealed class LexerException : JinjaException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LexerException"/> class with a specified error message and position.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="position">The position in the template where the error occurred.</param>
    public LexerException(string message, Position position)
        : base(message, position)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="LexerException"/> class with a specified error message, position, and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="position">The position in the template where the error occurred.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public LexerException(string message, Position position, Exception innerException)
        : base(message, position, innerException)
    {
    }
}