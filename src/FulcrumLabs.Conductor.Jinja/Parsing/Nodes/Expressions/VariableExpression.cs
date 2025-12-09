namespace FulcrumLabs.Conductor.Jinja.Parsing.Nodes.Expressions;

/// <summary>
///     Represents a variable reference.
/// </summary>
public sealed class VariableExpression : IExpression
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="VariableExpression"/> class.
    /// </summary>
    /// <param name="name">The variable name.</param>
    public VariableExpression(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     Gets the variable name.
    /// </summary>
    public string Name { get; init; }
}