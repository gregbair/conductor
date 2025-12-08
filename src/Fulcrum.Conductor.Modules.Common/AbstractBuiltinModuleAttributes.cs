namespace Fulcrum.Conductor.Modules.Common;

public abstract class AbstractBuiltinModuleAttributes : IModuleAttributes
{
    public abstract string[] RespondsTo { get; }
    public string Author => "Greg Bair";
    public Uri Url => new("https://codeberg.org/fulcrumlabs/conductor");
    public abstract string Description { get; }
}