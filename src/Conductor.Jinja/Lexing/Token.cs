using Conductor.Jinja.Common;

namespace Conductor.Jinja.Lexing;

/// <summary>
///     Represents a token in a Jinja2 template.
/// </summary>
public readonly struct Token(TokenType type, string value, Position position)
{
    /// <summary>
    ///     Gets the type of this token.
    /// </summary>
    public TokenType Type { get; init; } = type;

    /// <summary>
    ///     Gets the string value of this token.
    /// </summary>
    public string Value { get; init; } = value;

    /// <summary>
    ///     Gets the position of this token in the source template.
    /// </summary>
    public Position Position { get; init; } = position;

    /// <summary>
    ///     Initializes a new instance of <see cref="Token" />
    /// </summary>
    /// <param name="type"></param>
    /// <param name="position"></param>
    public Token(TokenType type, Position position)
        : this(type, string.Empty, position)
    {
    }

    public override string ToString()
    {
        return !string.IsNullOrEmpty(Value) ? $"{Type}('{Value}')" : Type.ToString();
    }
}