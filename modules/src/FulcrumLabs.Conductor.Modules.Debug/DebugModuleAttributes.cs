using FulcrumLabs.Conductor.Modules.Common;

namespace FulcrumLabs.Conductor.Modules.Debug;

public class DebugModuleAttributes : AbstractBuiltinModuleAttributes
{
    public override string[] RespondsTo { get; } =
    [
        "ansible.builtin.debug",
        "conductor.builtin.debug",
        "debug"
    ];

    public override string Description => "Prints messages useful for debugging";
}