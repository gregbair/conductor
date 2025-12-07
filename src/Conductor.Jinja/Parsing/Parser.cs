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
                return ParseBlockStatement();
            case TokenType.Eof:
                Advance();
                throw new ParserException("Unexpected end of file", token.Position);
            default:
                throw new ParserException($"Unexpected token: {token.Type}", token.Position);
        }
    }

    private IStatement ParseBlockStatement()
    {
        Consume(TokenType.BlockStart, "Expected '{%'");
        Token keyword = Peek();

        switch (keyword.Type)
        {
            case TokenType.For:
                return ParseForStatement();
            case TokenType.If:
                return ParseIfStatement();
            default:
                throw new ParserException($"Unexpected block keyword: {keyword.Type}", keyword.Position);
        }
    }

    private IStatement ParseForStatement()
    {
        Consume(TokenType.For, "Expected 'for'");
        Token loopVar = Consume(TokenType.Identifier, "Expected loop variable");
        Consume(TokenType.In, "Expected 'in'");
        IExpression iterable = ParseExpression();
        Consume(TokenType.BlockEnd, "Expected '%}'");

        List<IStatement> body = new();
        while (!IsAtEnd())
        {
            if (Peek().Type == TokenType.BlockStart && PeekAhead(1).Type == TokenType.Endfor)
            {
                break;
            }

            body.Add(ParseStatement());
        }

        Consume(TokenType.BlockStart, "Expected '{%'");
        Consume(TokenType.Endfor, "Expected 'endfor'");
        Consume(TokenType.BlockEnd, "Expected '%}'");

        return new ForStatement(loopVar.Value, iterable, body);
    }

    private Token PeekAhead(int offset)
    {
        int position = _current + offset;
        if (position >= _tokens.Count)
        {
            return _tokens[^1];
        }

        return _tokens[position];
    }

    private IStatement ParseIfStatement()
    {
        Consume(TokenType.If, "Expected 'if'");
        IExpression condition = ParseExpression();
        Consume(TokenType.BlockEnd, "Expected '%}'");

        List<IStatement> thenBody = ParseUntilBlockKeyword(TokenType.Elif, TokenType.Else, TokenType.Endif);

        List<(IExpression, IList<IStatement>)> elifBranches = new();
        IList<IStatement>? elseBody = null;

        while (Peek().Type == TokenType.BlockStart)
        {
            int savedPosition = _current;
            Consume(TokenType.BlockStart, "Expected '{%'");
            Token keyword = Peek();

            if (keyword.Type == TokenType.Elif)
            {
                Consume(TokenType.Elif, "Expected 'elif'");
                IExpression elifCondition = ParseExpression();
                Consume(TokenType.BlockEnd, "Expected '%}'");

                List<IStatement> elifBody = ParseUntilBlockKeyword(TokenType.Elif, TokenType.Else, TokenType.Endif);
                elifBranches.Add((elifCondition, elifBody));
            }
            else if (keyword.Type == TokenType.Else)
            {
                Consume(TokenType.Else, "Expected 'else'");
                Consume(TokenType.BlockEnd, "Expected '%}'");

                elseBody = ParseUntilBlockKeyword(TokenType.Endif);
                break;
            }
            else if (keyword.Type == TokenType.Endif)
            {
                _current = savedPosition;
                break;
            }
            else
            {
                throw new ParserException($"Unexpected token in if statement: {keyword.Type}", keyword.Position);
            }
        }

        Consume(TokenType.BlockStart, "Expected '{%'");
        Consume(TokenType.Endif, "Expected 'endif'");
        Consume(TokenType.BlockEnd, "Expected '%}'");

        return new IfStatement(condition, thenBody) { ElifBranches = elifBranches, ElseBody = elseBody };
    }

    private List<IStatement> ParseUntilBlockKeyword(params TokenType[] keywords)
    {
        List<IStatement> statements = new();

        while (!IsAtEnd())
        {
            if (Peek().Type == TokenType.BlockStart)
            {
                Token nextKeyword = PeekAhead(1);
                foreach (TokenType keyword in keywords)
                {
                    if (nextKeyword.Type == keyword)
                    {
                        return statements;
                    }
                }
            }

            statements.Add(ParseStatement());
        }

        return statements;
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
        return ParseLogicalOr();
    }

    private IExpression ParseLogicalOr()
    {
        IExpression left = ParseLogicalAnd();

        while (Peek().Type == TokenType.Or)
        {
            Advance();
            IExpression right = ParseLogicalAnd();
            left = new BinaryExpression(left, BinaryOperator.Or, right);
        }

        return left;
    }

    private IExpression ParseLogicalAnd()
    {
        IExpression left = ParseComparison();

        while (Peek().Type == TokenType.And)
        {
            Advance();
            IExpression right = ParseComparison();
            left = new BinaryExpression(left, BinaryOperator.And, right);
        }

        return left;
    }

    private IExpression ParseComparison()
    {
        IExpression left = ParseAdditive();

        TokenType type = Peek().Type;
        if (type == TokenType.Equal || type == TokenType.NotEqual ||
            type == TokenType.LessThan || type == TokenType.LessThanOrEqual ||
            type == TokenType.GreaterThan || type == TokenType.GreaterThanOrEqual ||
            type == TokenType.In)
        {
            Token op = Advance();
            IExpression right = ParseAdditive();

            BinaryOperator binaryOp = op.Type switch
            {
                TokenType.Equal => BinaryOperator.Equal,
                TokenType.NotEqual => BinaryOperator.NotEqual,
                TokenType.LessThan => BinaryOperator.LessThan,
                TokenType.LessThanOrEqual => BinaryOperator.LessThanOrEqual,
                TokenType.GreaterThan => BinaryOperator.GreaterThan,
                TokenType.GreaterThanOrEqual => BinaryOperator.GreaterThanOrEqual,
                TokenType.In => BinaryOperator.In,
                _ => throw new ParserException($"Unknown comparison operator: {op.Type}", op.Position)
            };

            left = new BinaryExpression(left, binaryOp, right);
        }

        return left;
    }

    private IExpression ParseAdditive()
    {
        IExpression left = ParseMultiplicative();

        while (Peek().Type == TokenType.Plus || Peek().Type == TokenType.Minus)
        {
            Token op = Advance();
            IExpression right = ParseMultiplicative();

            BinaryOperator binaryOp = op.Type == TokenType.Plus ? BinaryOperator.Add : BinaryOperator.Subtract;
            left = new BinaryExpression(left, binaryOp, right);
        }

        return left;
    }

    private IExpression ParseMultiplicative()
    {
        IExpression left = ParseUnary();

        while (Peek().Type == TokenType.Multiply || Peek().Type == TokenType.Divide ||
               Peek().Type == TokenType.FloorDivide || Peek().Type == TokenType.Modulo)
        {
            Token op = Advance();
            IExpression right = ParseUnary();

            BinaryOperator binaryOp = op.Type switch
            {
                TokenType.Multiply => BinaryOperator.Multiply,
                TokenType.Divide => BinaryOperator.Divide,
                TokenType.FloorDivide => BinaryOperator.FloorDivide,
                TokenType.Modulo => BinaryOperator.Modulo,
                _ => throw new ParserException($"Unknown multiplicative operator: {op.Type}", op.Position)
            };

            left = new BinaryExpression(left, binaryOp, right);
        }

        return left;
    }

    private IExpression ParseUnary()
    {
        Token token = Peek();

        if (token.Type == TokenType.Not)
        {
            Advance();
            IExpression operand = ParseUnary();
            return new UnaryExpression(UnaryOperator.Not, operand);
        }

        if (token.Type == TokenType.Minus)
        {
            Advance();
            IExpression operand = ParseUnary();
            return new UnaryExpression(UnaryOperator.Negate, operand);
        }

        return ParsePostfix();
    }

    private IExpression ParsePostfix()
    {
        IExpression expression = ParsePrimary();

        while (true)
        {
            Token token = Peek();

            if (token.Type == TokenType.Dot)
            {
                Advance();
                Token property = Consume(TokenType.Identifier, "Expected property name after '.'");
                expression = new MemberExpression(expression, property.Value);
            }
            else if (token.Type == TokenType.LeftBracket)
            {
                Advance();
                IExpression index = ParseExpression();
                Consume(TokenType.RightBracket, "Expected ']'");
                expression = new IndexExpression(expression, index);
            }
            else if (token.Type == TokenType.Pipe)
            {
                Advance();
                Token filterName = Consume(TokenType.Identifier, "Expected filter name after '|'");
                List<IExpression> arguments = new();

                if (Peek().Type == TokenType.LeftParen)
                {
                    Advance();

                    if (Peek().Type != TokenType.RightParen)
                    {
                        arguments.Add(ParseExpression());

                        while (Peek().Type == TokenType.Comma)
                        {
                            Advance();
                            arguments.Add(ParseExpression());
                        }
                    }

                    Consume(TokenType.RightParen, "Expected ')'");
                }

                expression = new FilterExpression(expression, filterName.Value, arguments);
            }
            else
            {
                break;
            }
        }

        return expression;
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