using FulcrumLabs.Conductor.Modules.Common;

namespace FulcrumLabs.Conductor.Modules.Debug;

/// <summary>
/// Attributes for the debug module.
/// </summary>
public class DebugModuleAttributes : AbstractBuiltinModuleAttributes
{
    /// <inheritdoc />
    public override string[] RespondsTo { get; } =
    [
        "ansible.builtin.debug",
        "conductor.builtin.debug",
        "debug"
    ];

    /// <inheritdoc />
    public override string Description => "Prints messages useful for debugging";
}