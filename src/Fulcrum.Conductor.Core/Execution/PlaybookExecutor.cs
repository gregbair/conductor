using Fulcrum.Conductor.Core.Playbooks;
using Fulcrum.Conductor.Core.Tasks;
using Fulcrum.Conductor.Jinja.Rendering;

using Task = Fulcrum.Conductor.Core.Tasks.Task;

namespace Fulcrum.Conductor.Core.Execution;

/// <summary>
///     Executes playbooks by running each play sequentially.
///     Manages variable scoping across plays.
/// </summary>
public sealed class PlaybookExecutor(ITaskExecutor taskExecutor) : IPlaybookExecutor
{
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

        // Execute each task sequentially
        foreach (Task task in play.Tasks)
        {
            try
            {
                TaskResult taskResult = await taskExecutor.ExecuteAsync(task, playContext, cancellationToken);
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
}