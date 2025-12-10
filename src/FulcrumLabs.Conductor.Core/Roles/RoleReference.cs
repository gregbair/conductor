namespace FulcrumLabs.Conductor.Core.Roles;

/// <summary>
/// Abstract base class for role references in playbooks.
/// </summary>
public abstract class RoleReference
{
    /// <summary>
    /// Gets the name of the role being referenced.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the parameters to pass to the role.
    /// </summary>
    public Dictionary<string, object?> Parameters { get; init; } = new();

    /// <summary>
    /// Gets the conditional expression that determines if this role should be executed.
    /// </summary>
    public string? When { get; init; }

    /// <summary>
    /// Gets the tags associated with this role.
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Represents a reference to an external role loaded from the roles directory.
/// </summary>
public sealed class ExternalRoleReference : RoleReference
{
}

/// <summary>
/// Represents an inline role defined directly in the playbook.
/// </summary>
public sealed class InlineRoleReference : RoleReference
{
    /// <summary>
    /// Gets the inline role definition.
    /// </summary>
    public required Role Role { get; init; }
}

/// <summary>
/// Represents a role imported using the import_role task.
/// Import is static and evaluated at parse time.
/// </summary>
public sealed class ImportRoleReference : RoleReference
{
}

/// <summary>
/// Represents a role included using the include_role task.
/// Include is dynamic and evaluated at runtime.
/// </summary>
public sealed class IncludeRoleReference : RoleReference
{
}
