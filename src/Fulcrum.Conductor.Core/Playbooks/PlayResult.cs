using TaskResult = Fulcrum.Conductor.Core.Tasks.TaskResult;

namespace Fulcrum.Conductor.Core.Playbooks;

/// <summary>
/// Represents the result of executing a single play.
/// </summary>
public sealed class PlayResult
{
    /// <summary>
    /// Name of the play.
    /// </summary>
    public string PlayName { get; init; } = string.Empty;

    /// <summary>
    /// Results from each task in the play.
    /// </summary>
    public IReadOnlyList<TaskResult> TaskResults { get; init; } = Array.Empty<TaskResult>();

    /// <summary>
    /// Whether the play executed successfully.
    /// </summary>
    public bool Success { get; init; }
}
