using Conductor.Jinja.Common;

namespace Conductor.Jinja.Lexing;

/// <summary>
///     Exception thrown when an error occurs during lexical analysis.
/// </summary>
public sealed class LexerException : JinjaException
{
    public LexerException(string message, Position position)
        : base(message, position)
    {
    }

    public LexerException(string message, Position position, Exception innerException)
        : base(message, position, innerException)
    {
    }
}