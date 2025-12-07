using System.Globalization;

using Conductor.Jinja.Lexing;
using Conductor.Jinja.Parsing.Nodes;
using Conductor.Jinja.Parsing.Nodes.Expressions;
using Conductor.Jinja.Parsing.Nodes.Statements;

namespace Conductor.Jinja.Parsing;

/// <summary>
///     Parser for Jinja2 templates.
/// </summary>
public sealed class Parser
{
    private int _current;
    private IList<Token> _tokens = new List<Token>();

    /// <summary>
    ///     Parses a list of tokens into an AST.
    /// </summary>
    public IList<IStatement> Parse(IList<Token> tokens)
    {
        _tokens = tokens;
        _current = 0;

        List<IStatement> statements = new();

        while (!IsAtEnd())
        {
            statements.Add(ParseStatement());
        }

        return statements;
    }

    private IStatement ParseStatement()
    {
        Token token = Peek();

        switch (token.Type)
        {
            case TokenType.Text:
                return ParseTextStatement();
            case TokenType.VariableStart:
                return ParseOutputStatement();
            case TokenType.BlockStart:
                throw new ParserException("Block statements not yet implemented", token.Position);
            case TokenType.Eof:
                Advance();
                throw new ParserException("Unexpected end of file", token.Position);
            default:
                throw new ParserException($"Unexpected token: {token.Type}", token.Position);
        }
    }

    private IStatement ParseTextStatement()
    {
        Token token = Consume(TokenType.Text, "Expected text");
        return new TextStatement(token.Value);
    }

    private IStatement ParseOutputStatement()
    {
        Consume(TokenType.VariableStart, "Expected '{{'");
        IExpression expression = ParseExpression();
        Consume(TokenType.VariableEnd, "Expected '}}'");

        return new OutputStatement(expression);
    }

    private IExpression ParseExpression()
    {
        return ParsePrimary();
    }

    private IExpression ParsePrimary()
    {
        Token token = Peek();

        switch (token.Type)
        {
            // Identifier/variable
            case TokenType.Identifier:
                Advance();
                return new VariableExpression(token.Value);
            // String literal
            case TokenType.String:
                Advance();
                return new LiteralExpression(token.Value);
            // Number literal
            case TokenType.Number:
                {
                    Advance();
                    if (double.TryParse(token.Value, CultureInfo.InvariantCulture, out double value))
                    {
                        return new LiteralExpression(value);
                    }

                    throw new ParserException($"Invalid number: {token.Value}", token.Position);
                }
            // Boolean literals
            case TokenType.True:
                Advance();
                return new LiteralExpression(true);
            case TokenType.False:
                Advance();
                return new LiteralExpression(false);
            // None/null literal
            case TokenType.None:
                Advance();
                return new LiteralExpression(null);
            // Parenthesized expression
            case TokenType.LeftParen:
                {
                    Advance();
                    IExpression expression = ParseExpression();
                    Consume(TokenType.RightParen, "Expected ')'");
                    return expression;
                }
            default:
                throw new ParserException($"Unexpected token in expression: {token.Type}", token.Position);
        }
    }

    private Token Peek()
    {
        return IsAtEnd() ? _tokens[^1] : _tokens[_current];
    }


    private Token Advance()
    {
        if (!IsAtEnd())
        {
            _current++;
        }

        return _tokens[_current - 1];
    }

    private Token Consume(TokenType type, string message)
    {
        Token token = Peek();
        return token.Type != type
            ? throw new ParserException($"{message}, got {token.Type}", token.Position)
            : Advance();
    }

    private bool IsAtEnd()
    {
        return _current >= _tokens.Count || _tokens[_current].Type == TokenType.Eof;
    }
}