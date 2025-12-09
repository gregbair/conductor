using FulcrumLabs.Conductor.Core.Playbooks;
using FulcrumLabs.Conductor.Core.Roles;
using FulcrumLabs.Conductor.Core.Tasks;
using FulcrumLabs.Conductor.Jinja.Rendering;

using Task = FulcrumLabs.Conductor.Core.Tasks.Task;

namespace FulcrumLabs.Conductor.Core.Execution;

/// <summary>
///     Executes playbooks by running each play sequentially.
///     Manages variable scoping across plays.
/// </summary>
public sealed class PlaybookExecutor(ITaskExecutor taskExecutor, IRoleExpander roleExpander) : IPlaybookExecutor
{
    private readonly ITaskExecutor _taskExecutor = taskExecutor;
    private readonly IRoleExpander _roleExpander = roleExpander;

    /// <inheritdoc/>
    public async Task<PlaybookResult> ExecuteAsync(
        Playbook playbook,
        TemplateContext? initialContext = null,
        CancellationToken cancellationToken = default)
    {
        // Create or use provided initial context
        TemplateContext context = initialContext ?? TemplateContext.Create();
        List<PlayResult> playResults = [];
        bool overallSuccess = true;

        foreach (Play play in playbook.Plays)
        {
            PlayResult playResult = await ExecutePlayAsync(play, context, cancellationToken);
            playResults.Add(playResult);

            if (playResult.Success)
            {
                continue;
            }

            overallSuccess = false;
            break; // Stop on first failed play
        }

        return new PlaybookResult { Success = overallSuccess, PlayResults = playResults };
    }

    private async Task<PlayResult> ExecutePlayAsync(
        Play play,
        TemplateContext parentContext,
        CancellationToken cancellationToken)
    {
        // Create play-scoped context (inherits from parent)
        TemplateContext playContext = parentContext.CreateChildScope();

        // Merge play vars into play context
        foreach ((string key, object? value) in play.Vars)
        {
            playContext.SetVariable(key, value);
        }

        List<TaskResult> taskResults = [];
        bool playSuccess = true;

        // Execute roles first (roles are executed before tasks)
        foreach (RoleReference roleRef in play.Roles)
        {
            List<TaskResult> roleResults = await ExecuteRoleAsync(roleRef, playContext, cancellationToken);
            taskResults.AddRange(roleResults);

            // If any role task failed, stop the play
            if (roleResults.Any(r => r is { Success: false, Skipped: false }))
            {
                playSuccess = false;
                return new PlayResult { PlayName = play.Name, TaskResults = taskResults, Success = playSuccess };
            }
        }

        // Execute each task sequentially
        foreach (Task task in play.Tasks)
        {
            try
            {
                TaskResult taskResult = await _taskExecutor.ExecuteAsync(task, playContext, cancellationToken);
                taskResults.Add(taskResult);

                // If task failed and wasn't skipped or ignored, stop the play
                if (taskResult is not { Success: false, Skipped: false })
                {
                    continue;
                }

                playSuccess = false;
                break;
            }
            catch (TaskExecutionException ex)
            {
                // Task execution failed critically
                TaskResult failedResult = new()
                {
                    Success = false,
                    Changed = false,
                    Failed = true,
                    Skipped = false,
                    Message = $"Task execution failed: {ex.Message}"
                };
                taskResults.Add(failedResult);
                playSuccess = false;
                break;
            }
        }

        return new PlayResult { PlayName = play.Name, TaskResults = taskResults, Success = playSuccess };
    }

    /// <summary>
    /// Executes a single role within a play.
    /// </summary>
    /// <param name="roleRef">The role reference to execute.</param>
    /// <param name="playContext">The play context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of task results from executing the role's tasks.</returns>
    private async Task<List<TaskResult>> ExecuteRoleAsync(
        RoleReference roleRef,
        TemplateContext playContext,
        CancellationToken cancellationToken)
    {
        List<TaskResult> roleTaskResults = [];

        // Expand the role reference
        RoleExpansion expansion = _roleExpander.ExpandRole(roleRef, playContext);

        // If role was skipped due to conditionals, return empty results
        if (expansion.Skipped || expansion.Role == null)
        {
            return roleTaskResults;
        }

        Role role = expansion.Role;

        // Create role-scoped context with proper variable precedence
        TemplateContext roleContext = CreateRoleContext(role, playContext);

        // Execute each task in the role
        foreach (Task task in role.Tasks)
        {
            try
            {
                TaskResult taskResult = await _taskExecutor.ExecuteAsync(task, roleContext, cancellationToken);
                roleTaskResults.Add(taskResult);

                // If task failed and wasn't skipped or ignored, stop the role
                if (taskResult is { Success: false, Skipped: false })
                {
                    break;
                }
            }
            catch (TaskExecutionException ex)
            {
                // Task execution failed critically
                TaskResult failedResult = new()
                {
                    Success = false,
                    Changed = false,
                    Failed = true,
                    Skipped = false,
                    Message = $"Task execution failed in role '{role.Name}': {ex.Message}"
                };
                roleTaskResults.Add(failedResult);
                break;
            }
        }

        return roleTaskResults;
    }

    /// <summary>
    /// Creates a role-scoped context with proper variable precedence.
    /// Precedence: play vars > role vars > role defaults
    /// </summary>
    /// <param name="role">The role to create context for.</param>
    /// <param name="playContext">The parent play context.</param>
    /// <returns>A new role-scoped template context.</returns>
    private static TemplateContext CreateRoleContext(Role role, TemplateContext playContext)
    {
        // Create child scope from play context (inherits play vars)
        TemplateContext roleContext = playContext.CreateChildScope();

        // Set role defaults (lowest precedence)
        // Only set if not already defined in play context
        foreach ((string key, object? value) in role.Defaults)
        {
            if (!playContext.IsDefined(key))
            {
                roleContext.SetVariable(key, value);
            }
        }

        // Set role vars (higher precedence than defaults)
        // Only set if not already defined in play context
        foreach ((string key, object? value) in role.Vars)
        {
            if (!playContext.IsDefined(key))
            {
                roleContext.SetVariable(key, value);
            }
        }

        // Set role parameters (highest precedence among role variables)
        // Only set if not already defined in play context
        foreach ((string key, object? value) in role.Parameters)
        {
            if (!playContext.IsDefined(key))
            {
                roleContext.SetVariable(key, value);
            }
        }

        return roleContext;
    }
}