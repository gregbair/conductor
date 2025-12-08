using ConductorTask = Fulcrum.Conductor.Core.Tasks.Task;

namespace Fulcrum.Conductor.Core.Playbooks;

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
    /// List of tasks in this play.
    /// </summary>
    public IReadOnlyList<ConductorTask> Tasks { get; init; } = Array.Empty<ConductorTask>();

    /// <summary>
    /// Variables defined at the play level.
    /// </summary>
    public Dictionary<string, object?> Vars { get; init; } = new();
}
