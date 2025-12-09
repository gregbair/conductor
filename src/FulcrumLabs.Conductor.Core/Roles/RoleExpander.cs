using FulcrumLabs.Conductor.Core.Conditionals;
using FulcrumLabs.Conductor.Jinja.Rendering;

namespace FulcrumLabs.Conductor.Core.Roles;

/// <summary>
/// Expands role references into concrete roles, handling conditionals and loading external roles.
/// </summary>
public sealed class RoleExpander : IRoleExpander
{
    private readonly IRoleLoader _roleLoader;
    private readonly IConditionalEvaluator _conditionalEvaluator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleExpander"/> class.
    /// </summary>
    /// <param name="roleLoader">The role loader for loading external roles.</param>
    /// <param name="conditionalEvaluator">The conditional evaluator for evaluating when conditions.</param>
    public RoleExpander(IRoleLoader roleLoader, IConditionalEvaluator conditionalEvaluator)
    {
        _roleLoader = roleLoader;
        _conditionalEvaluator = conditionalEvaluator;
    }

    /// <inheritdoc />
    public RoleExpansion ExpandRole(RoleReference roleRef, TemplateContext context)
    {
        try
        {
            // Evaluate when condition if present
            if (!string.IsNullOrWhiteSpace(roleRef.When))
            {
                bool conditionMet = _conditionalEvaluator.Evaluate(roleRef.When, context);
                if (!conditionMet)
                {
                    return new RoleExpansion { Role = null, Skipped = true };
                }
            }

            // Expand based on reference type
            Role role = roleRef switch
            {
                ExternalRoleReference externalRef => LoadExternalRole(externalRef),
                InlineRoleReference inlineRef => inlineRef.Role,
                ImportRoleReference importRef => LoadExternalRole(importRef),
                IncludeRoleReference includeRef => LoadExternalRole(includeRef),
                _ => throw new RoleExpansionException($"Unknown role reference type: {roleRef.GetType().Name}")
            };

            // Merge role parameters into the role
            if (roleRef.Parameters.Count > 0)
            {
                role = new Role
                {
                    Name = role.Name,
                    Tasks = role.Tasks,
                    Defaults = role.Defaults,
                    Vars = role.Vars,
                    Parameters = roleRef.Parameters
                };
            }

            return new RoleExpansion { Role = role, Skipped = false };
        }
        catch (RoleExpansionException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new RoleExpansionException($"Failed to expand role '{roleRef.Name}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Loads an external role from the filesystem.
    /// </summary>
    /// <param name="roleRef">The role reference containing the role name.</param>
    /// <returns>The loaded role.</returns>
    private Role LoadExternalRole(RoleReference roleRef)
    {
        try
        {
            return _roleLoader.LoadRole(roleRef.Name);
        }
        catch (RoleLoadException ex)
        {
            throw new RoleExpansionException($"Failed to load external role '{roleRef.Name}': {ex.Message}", ex);
        }
    }
}
