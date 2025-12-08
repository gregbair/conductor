namespace Fulcrum.Conductor.Core.Playbooks;

/// <summary>
/// Represents a complete playbook.
/// Immutable after construction.
/// </summary>
public sealed class Playbook
{
    /// <summary>
    /// List of plays in this playbook.
    /// </summary>
    public IReadOnlyList<Play> Plays { get; init; } = Array.Empty<Play>();
}
