using ConductorTask = FulcrumLabs.Conductor.Core.Tasks.Task;
using Task = FulcrumLabs.Conductor.Core.Tasks.Task;
using FulcrumLabs.Conductor.Core.Roles;

namespace FulcrumLabs.Conductor.Core.Playbooks;

/// <summary>
/// Represents a single play within a playbook.
/// A play is a collection of tasks to be executed.
/// </summary>
public sealed class Play
{
    /// <summary>
    /// Human-readable name of the play.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// List of role references to be executed in this play.
    /// Roles are executed before tasks.
    /// </summary>
    public IReadOnlyList<RoleReference> Roles { get; init; } = Array.Empty<RoleReference>();

    /// <summary>
    /// List of tasks in this play.
    /// </summary>
    public IReadOnlyList<Task> Tasks { get; init; } = Array.Empty<Task>();

    /// <summary>
    /// Variables defined at the play level.
    /// </summary>
    public Dictionary<string, object?> Vars { get; init; } = new();
}
