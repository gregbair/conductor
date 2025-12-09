namespace FulcrumLabs.Conductor.Jinja.Parsing.Nodes.Expressions;

/// <summary>
///     Represents index access (e.g., items[0], dict['key']).
/// </summary>
public sealed class IndexExpression : IExpression
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="IndexExpression"/> class.
    /// </summary>
    /// <param name="obj">The object being indexed.</param>
    /// <param name="index">The index expression.</param>
    public IndexExpression(IExpression obj, IExpression index)
    {
        Object = obj;
        Index = index;
    }

    /// <summary>
    ///     Gets the object being indexed.
    /// </summary>
    public IExpression Object { get; init; }

    /// <summary>
    ///     Gets the index expression.
    /// </summary>
    public IExpression Index { get; init; }
}