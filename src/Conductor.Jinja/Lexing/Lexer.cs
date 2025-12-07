using System.Text;

using Conductor.Jinja.Common;

namespace Conductor.Jinja.Lexing;

/// <summary>
///     Lexical analyzer for Jinja2 templates.
/// </summary>
public sealed class Lexer
{
    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        ["for"] = TokenType.For,
        ["endfor"] = TokenType.Endfor,
        ["if"] = TokenType.If,
        ["elif"] = TokenType.Elif,
        ["else"] = TokenType.Else,
        ["endif"] = TokenType.Endif,
        ["set"] = TokenType.Set,
        ["endset"] = TokenType.Endset,
        ["in"] = TokenType.In,
        ["is"] = TokenType.Is,
        ["and"] = TokenType.And,
        ["or"] = TokenType.Or,
        ["not"] = TokenType.Not,
        ["true"] = TokenType.True,
        ["false"] = TokenType.False,
        ["none"] = TokenType.None,
        ["null"] = TokenType.None
    };

    private int _column;
    private int _index;
    private bool _inExpression;
    private int _line;
    private string _template = string.Empty;

    /// <summary>
    ///     Tokenizes the given template.
    /// </summary>
    public IList<Token> Tokenize(string template)
    {
        _template = template;
        _index = 0;
        _line = 1;
        _column = 1;
        _inExpression = false;

        List<Token> tokens = new();

        while (!IsAtEnd())
        {
            tokens.Add(ScanToken());
        }

        tokens.Add(CreateToken(TokenType.Eof, string.Empty));
        return tokens;
    }

    private Token ScanToken()
    {
        return _inExpression ? ScanExpressionToken() : ScanTextOrDelimiter();
    }

    private Token ScanTextOrDelimiter()
    {
        Position start = CurrentPosition();

        if (Peek() != '{')
        {
            return ScanText(start);
        }

        if (PeekAhead(1) == '{')
        {
            Advance(); // {
            Advance(); // {
            _inExpression = true;
            return CreateToken(TokenType.VariableStart, "{{", start);
        }

        if (PeekAhead(1) == '%')
        {
            Advance(); // {
            Advance(); // %
            _inExpression = true;
            return CreateToken(TokenType.BlockStart, "{%", start);
        }

        if (PeekAhead(1) == '#')
        {
            Advance(); // {
            Advance(); // #
            return ScanComment(start);
        }

        return ScanText(start);
    }

    private Token ScanText(Position start)
    {
        StringBuilder text = new();

        while (!IsAtEnd())
        {
            if (Peek() == '{' && (PeekAhead(1) == '{' || PeekAhead(1) == '%' || PeekAhead(1) == '#'))
            {
                break;
            }

            text.Append(Advance());
        }

        return CreateToken(TokenType.Text, text.ToString(), start);
    }

    private Token ScanComment(Position start)
    {
        StringBuilder comment = new();

        while (!IsAtEnd())
        {
            if (Peek() == '#' && PeekAhead(1) == '}')
            {
                Advance(); // #
                Advance(); // }
                break;
            }

            comment.Append(Advance());
        }

        return CreateToken(TokenType.Text, string.Empty, start);
    }

    private Token ScanExpressionToken()
    {
        SkipWhitespace();

        if (IsAtEnd())
        {
            return CreateToken(TokenType.Eof, string.Empty);
        }

        Position start = CurrentPosition();
        char current = Peek();

        switch (current)
        {
            // Check for closing delimiters
            case '}' when PeekAhead(1) == '}':
                Advance(); // }
                Advance(); // }
                _inExpression = false;
                return CreateToken(TokenType.VariableEnd, "}}", start);
            case '%' when PeekAhead(1) == '}':
                Advance(); // %
                Advance(); // }
                _inExpression = false;
                return CreateToken(TokenType.BlockEnd, "%}", start);
        }

        // Single-character tokens
        switch (current)
        {
            case '.':
                Advance();
                return CreateToken(TokenType.Dot, ".", start);
            case '|':
                Advance();
                return CreateToken(TokenType.Pipe, "|", start);
            case ',':
                Advance();
                return CreateToken(TokenType.Comma, ",", start);
            case ':':
                Advance();
                return CreateToken(TokenType.Colon, ":", start);
            case '(':
                Advance();
                return CreateToken(TokenType.LeftParen, "(", start);
            case ')':
                Advance();
                return CreateToken(TokenType.RightParen, ")", start);
            case '[':
                Advance();
                return CreateToken(TokenType.LeftBracket, "[", start);
            case ']':
                Advance();
                return CreateToken(TokenType.RightBracket, "]", start);
            case '{':
                Advance();
                return CreateToken(TokenType.LeftBrace, "{", start);
            case '}':
                Advance();
                return CreateToken(TokenType.RightBrace, "}", start);
            case '+':
                Advance();
                return CreateToken(TokenType.Plus, "+", start);
            case '-':
                Advance();
                return CreateToken(TokenType.Minus, "-", start);
            case '%':
                Advance();
                return CreateToken(TokenType.Modulo, "%", start);
        }

        switch (current)
        {
            // Multi-character operators
            case '*':
                {
                    Advance();
                    if (Peek() != '*')
                    {
                        return CreateToken(TokenType.Multiply, "*", start);
                    }

                    Advance();
                    return CreateToken(TokenType.Power, "**", start);
                }
            case '/':
                {
                    Advance();
                    if (Peek() == '/')
                    {
                        Advance();
                        return CreateToken(TokenType.FloorDivide, "//", start);
                    }

                    return CreateToken(TokenType.Divide, "/", start);
                }
            case '=':
                {
                    Advance();
                    if (Peek() != '=')
                    {
                        return CreateToken(TokenType.Assign, "=", start);
                    }

                    Advance();
                    return CreateToken(TokenType.Equal, "==", start);
                }
            case '!':
                {
                    Advance();
                    if (Peek() != '=')
                    {
                        throw new LexerException("Unexpected character '!'", start);
                    }

                    Advance();
                    return CreateToken(TokenType.NotEqual, "!=", start);
                }
            case '<':
                {
                    Advance();
                    if (Peek() == '=')
                    {
                        Advance();
                        return CreateToken(TokenType.LessThanOrEqual, "<=", start);
                    }

                    return CreateToken(TokenType.LessThan, "<", start);
                }
            case '>':
                {
                    Advance();
                    if (Peek() == '=')
                    {
                        Advance();
                        return CreateToken(TokenType.GreaterThanOrEqual, ">=", start);
                    }

                    return CreateToken(TokenType.GreaterThan, ">", start);
                }
            // String literals
            case '\'':
            case '"':
                return ScanString(start);
        }

        // Numeric literals
        if (char.IsDigit(current))
        {
            return ScanNumber(start);
        }

        // Identifiers and keywords
        if (char.IsLetter(current) || current == '_')
        {
            return ScanIdentifierOrKeyword(start);
        }

        throw new LexerException($"Unexpected character '{current}'", start);
    }

    private Token ScanString(Position start)
    {
        char quote = Advance();
        StringBuilder value = new();

        while (!IsAtEnd() && Peek() != quote)
        {
            if (Peek() == '\\' && PeekAhead(1) == quote)
            {
                Advance(); // \
                value.Append(Advance()); // quote
            }
            else if (Peek() == '\\' && PeekAhead(1) == '\\')
            {
                Advance(); // \
                value.Append(Advance()); // \
            }
            else if (Peek() == '\\' && PeekAhead(1) == 'n')
            {
                Advance(); // \
                Advance(); // n
                value.Append('\n');
            }
            else if (Peek() == '\\' && PeekAhead(1) == 't')
            {
                Advance(); // \
                Advance(); // t
                value.Append('\t');
            }
            else
            {
                value.Append(Advance());
            }
        }

        if (IsAtEnd())
        {
            throw new LexerException("Unterminated string", start);
        }

        Advance(); // closing quote

        return CreateToken(TokenType.String, value.ToString(), start);
    }

    private Token ScanNumber(Position start)
    {
        StringBuilder value = new();

        while (!IsAtEnd() && char.IsDigit(Peek()))
        {
            value.Append(Advance());
        }

        if (Peek() != '.' || !char.IsDigit(PeekAhead(1)))
        {
            return CreateToken(TokenType.Number, value.ToString(), start);
        }

        value.Append(Advance()); // .

        while (!IsAtEnd() && char.IsDigit(Peek()))
        {
            value.Append(Advance());
        }

        return CreateToken(TokenType.Number, value.ToString(), start);
    }

    private Token ScanIdentifierOrKeyword(Position start)
    {
        StringBuilder value = new();

        while (!IsAtEnd() && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
        {
            value.Append(Advance());
        }

        string identifier = value.ToString();

        if (Keywords.TryGetValue(identifier, out TokenType keywordType))
        {
            return CreateToken(keywordType, identifier, start);
        }

        return CreateToken(TokenType.Identifier, identifier, start);
    }

    private void SkipWhitespace()
    {
        while (!IsAtEnd() && char.IsWhiteSpace(Peek()))
        {
            Advance();
        }
    }

    private char Advance()
    {
        char current = _template[_index];
        _index++;

        if (current == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }

        return current;
    }

    private char Peek()
    {
        if (IsAtEnd())
        {
            return '\0';
        }

        return _template[_index];
    }

    private char PeekAhead(int offset)
    {
        int position = _index + offset;
        if (position >= _template.Length)
        {
            return '\0';
        }

        return _template[position];
    }

    private bool IsAtEnd()
    {
        return _index >= _template.Length;
    }

    private Position CurrentPosition()
    {
        return new Position(_line, _column, _index);
    }

    private Token CreateToken(TokenType type, string value)
    {
        return new Token(type, value, CurrentPosition());
    }

    private static Token CreateToken(TokenType type, string value, Position position)
    {
        return new Token(type, value, position);
    }
}