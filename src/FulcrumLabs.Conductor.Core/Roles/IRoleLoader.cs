namespace FulcrumLabs.Conductor.Core.Roles;

/// <summary>
/// Interface for loading roles from the filesystem.
/// </summary>
public interface IRoleLoader
{
    /// <summary>
    /// Loads a role from the filesystem.
    /// </summary>
    /// <param name="roleName">The name of the role to load.</param>
    /// <param name="rolesBasePath">The base path where roles are stored. Defaults to "./roles".</param>
    /// <returns>The loaded role.</returns>
    /// <exception cref="RoleLoadException">Thrown when the role cannot be loaded.</exception>
    Role LoadRole(string roleName, string rolesBasePath = "./roles");
}
