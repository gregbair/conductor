namespace Fulcrum.Conductor.Core.Playbooks;

/// <summary>
/// Represents the result of executing a playbook.
/// </summary>
public sealed class PlaybookResult
{
    /// <summary>
    /// Whether the entire playbook executed successfully.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Results from each play in the playbook.
    /// </summary>
    public IReadOnlyList<PlayResult> PlayResults { get; init; } = Array.Empty<PlayResult>();
}
