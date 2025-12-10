namespace FulcrumLabs.Conductor.Core.Roles;

/// <summary>
/// Represents the result of expanding a role reference.
/// </summary>
public sealed class RoleExpansion
{
    /// <summary>
    /// Gets the expanded role, or null if the role was skipped due to conditionals.
    /// </summary>
    public Role? Role { get; init; }

    /// <summary>
    /// Gets a value indicating whether the role was skipped due to conditional evaluation.
    /// </summary>
    public bool Skipped { get; init; }
}
