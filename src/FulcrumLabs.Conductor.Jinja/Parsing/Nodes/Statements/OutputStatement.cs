namespace FulcrumLabs.Conductor.Jinja.Parsing.Nodes.Statements;

/// <summary>
///     Represents an output expression in the template ({{ expression }}).
/// </summary>
public sealed class OutputStatement : IStatement
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="OutputStatement"/> class.
    /// </summary>
    /// <param name="expression">The expression to evaluate and output.</param>
    public OutputStatement(IExpression expression)
    {
        Expression = expression;
    }

    /// <summary>
    ///     Gets the expression to evaluate and output.
    /// </summary>
    public IExpression Expression { get; init; }
}