namespace FulcrumLabs.Conductor.Jinja.Parsing.Nodes.Statements;

/// <summary>
///     Represents an output expression in the template ({{ expression }}).
/// </summary>
public sealed class OutputStatement : IStatement
{
    public OutputStatement(IExpression expression)
    {
        Expression = expression;
    }

    /// <summary>
    ///     Gets the expression to evaluate and output.
    /// </summary>
    public IExpression Expression { get; init; }
}