namespace Fulcrum.Conductor.Jinja.Parsing.Nodes.Expressions;

/// <summary>
///     Represents a variable reference.
/// </summary>
public sealed class VariableExpression : IExpression
{
    public VariableExpression(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     Gets the variable name.
    /// </summary>
    public string Name { get; init; }
}