namespace Fulcrum.Conductor.Jinja.Parsing.Nodes.Statements;

/// <summary>
///     Represents a for loop in a Jinja2 template.
/// </summary>
public sealed class ForStatement : IStatement
{
    public ForStatement(string loopVariable, IExpression iterable, IList<IStatement> body)
    {
        LoopVariable = loopVariable;
        Iterable = iterable;
        Body = body;
    }

    /// <summary>
    ///     Gets the name of the loop variable.
    /// </summary>
    public string LoopVariable { get; init; }

    /// <summary>
    ///     Gets the expression that provides the items to iterate over.
    /// </summary>
    public IExpression Iterable { get; init; }

    /// <summary>
    ///     Gets the statements in the loop body.
    /// </summary>
    public IList<IStatement> Body { get; init; }
}