namespace FulcrumLabs.Conductor.Jinja.Parsing.Nodes.Statements;

/// <summary>
///     Represents an if/elif/else conditional in a Jinja2 template.
/// </summary>
public sealed class IfStatement : IStatement
{
    public IfStatement(IExpression condition, IList<IStatement> thenBody)
    {
        Condition = condition;
        ThenBody = thenBody;
        ElifBranches = new List<(IExpression Condition, IList<IStatement> Body)>();
        ElseBody = null;
    }

    /// <summary>
    ///     Gets the condition expression.
    /// </summary>
    public IExpression Condition { get; init; }

    /// <summary>
    ///     Gets the statements to execute if the condition is true.
    /// </summary>
    public IList<IStatement> ThenBody { get; init; }

    /// <summary>
    ///     Gets the elif branches (condition and body pairs).
    /// </summary>
    public IList<(IExpression Condition, IList<IStatement> Body)> ElifBranches { get; init; }

    /// <summary>
    ///     Gets the else body (executed if all conditions are false).
    /// </summary>
    public IList<IStatement>? ElseBody { get; init; }
}