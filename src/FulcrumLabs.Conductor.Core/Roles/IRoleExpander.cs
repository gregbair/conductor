using FulcrumLabs.Conductor.Jinja.Rendering;

namespace FulcrumLabs.Conductor.Core.Roles;

/// <summary>
/// Interface for expanding role references into concrete roles.
/// </summary>
public interface IRoleExpander
{
    /// <summary>
    /// Expands a role reference into a concrete role.
    /// </summary>
    /// <param name="roleRef">The role reference to expand.</param>
    /// <param name="context">The template context for variable expansion and conditional evaluation.</param>
    /// <returns>A <see cref="RoleExpansion"/> containing the expanded role or indicating the role was skipped.</returns>
    /// <exception cref="RoleExpansionException">Thrown when the role cannot be expanded.</exception>
    RoleExpansion ExpandRole(RoleReference roleRef, TemplateContext context);
}
