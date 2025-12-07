namespace Conductor.Jinja.Parsing.Nodes.Expressions;

/// <summary>
///     Represents member access (e.g., user.name, obj.property).
/// </summary>
public sealed class MemberExpression : IExpression
{
    public MemberExpression(IExpression obj, string propertyName)
    {
        Object = obj;
        PropertyName = propertyName;
    }

    /// <summary>
    ///     Gets the object being accessed.
    /// </summary>
    public IExpression Object { get; init; }

    /// <summary>
    ///     Gets the name of the property being accessed.
    /// </summary>
    public string PropertyName { get; init; }
}