using Conductor.Jinja.Common;

namespace Conductor.Jinja.Parsing;

/// <summary>
///     Exception thrown when an error occurs during parsing.
/// </summary>
public sealed class ParserException : JinjaException
{
    public ParserException(string message, Position position)
        : base(message, position)
    {
    }

    public ParserException(string message, Position position, Exception innerException)
        : base(message, position, innerException)
    {
    }
}