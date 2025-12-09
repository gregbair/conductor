namespace FulcrumLabs.Conductor.Jinja.Parsing.Nodes.Statements;

/// <summary>
///     Represents a for loop in a Jinja2 template.
/// </summary>
public sealed class ForStatement : IStatement
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ForStatement"/> class.
    /// </summary>
    /// <param name="loopVariable">The name of the loop variable.</param>
    /// <param name="iterable">The expression that provides the items to iterate over.</param>
    /// <param name="body">The statements in the loop body.</param>
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