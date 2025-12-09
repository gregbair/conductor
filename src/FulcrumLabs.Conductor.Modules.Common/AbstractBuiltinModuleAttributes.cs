namespace FulcrumLabs.Conductor.Modules.Common;

/// <summary>
/// Abstract base class for built-in module attributes.
/// </summary>
public abstract class AbstractBuiltinModuleAttributes : IModuleAttributes
{
    /// <inheritdoc />
    public abstract string[] RespondsTo { get; }

    /// <inheritdoc />
    public string Author => "Greg Bair";

    /// <inheritdoc />
    public Uri Url => new("https://github.com/gregbair/conductor");

    /// <inheritdoc />
    public abstract string Description { get; }
}