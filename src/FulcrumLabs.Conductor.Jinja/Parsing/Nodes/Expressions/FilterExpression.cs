namespace FulcrumLabs.Conductor.Jinja.Parsing.Nodes.Expressions;

/// <summary>
///     Represents a filter expression (e.g., value | filter or value | filter(arg1, arg2)).
/// </summary>
public sealed class FilterExpression : IExpression
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FilterExpression"/> class.
    /// </summary>
    /// <param name="value">The value being filtered.</param>
    /// <param name="filterName">The name of the filter.</param>
    /// <param name="arguments">The arguments passed to the filter.</param>
    public FilterExpression(IExpression value, string filterName, IList<IExpression> arguments)
    {
        Value = value;
        FilterName = filterName;
        Arguments = arguments;
    }

    /// <summary>
    ///     Gets the value being filtered.
    /// </summary>
    public IExpression Value { get; init; }

    /// <summary>
    ///     Gets the name of the filter.
    /// </summary>
    public string FilterName { get; init; }

    /// <summary>
    ///     Gets the arguments passed to the filter.
    /// </summary>
    public IList<IExpression> Arguments { get; init; }
}