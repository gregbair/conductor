using FulcrumLabs.Conductor.Modules.Common;

namespace FulcrumLabs.Conductor.Modules.Shell;

/// <summary>
/// Attributes for the shell module.
/// </summary>
public class ShellModuleAttributes : AbstractBuiltinModuleAttributes
{
    /// <inheritdoc />
    public override string[] RespondsTo { get; } =
    [
        "ansible.builtin.shell",
        "ansible.builtin.command",
        "conductor.builtin.shell",
        "shell",
        "command"
    ];

    /// <inheritdoc />
    public override string Description => "Executes shell commands on the target host";
}