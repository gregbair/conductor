namespace FulcrumLabs.Conductor.Core.Roles;

/// <summary>
/// Represents an Ansible-compatible role containing tasks and variables.
/// </summary>
public sealed class Role
{
    /// <summary>
    /// Gets the name of the role.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the list of tasks defined in this role.
    /// </summary>
    public required IReadOnlyList<Tasks.Task> Tasks { get; init; }

    /// <summary>
    /// Gets the default variables for this role (lowest precedence).
    /// </summary>
    public required Dictionary<string, object?> Defaults { get; init; }

    /// <summary>
    /// Gets the role variables (higher precedence than defaults).
    /// </summary>
    public required Dictionary<string, object?> Vars { get; init; }

    /// <summary>
    /// Gets the parameters passed to this role when invoked.
    /// </summary>
    public required Dictionary<string, object?> Parameters { get; init; }
}
