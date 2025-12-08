using Fulcrum.Conductor.Modules.Common;

namespace Fulcrum.Conductor.Modules.Shell;

public class ShellModuleAttributes : AbstractBuiltinModuleAttributes
{
    public override string[] RespondsTo { get; } =
    [
        "ansible.builtin.shell",
        "ansible.builtin.command",
        "conductor.builtin.shell",
        "shell",
        "command"
    ];

    public override string Description => "Executes shell commands on the target host";
}