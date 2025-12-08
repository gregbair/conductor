using FulcrumLabs.Conductor.Jinja.Rendering;

using FulcrumLabs.Conductor.Core.Tasks;

namespace FulcrumLabs.Conductor.Core.Execution;

/// <summary>
/// Executes a single task.
/// </summary>
public interface ITaskExecutor
{
    /// <summary>
    /// Executes a task with the given context.
    /// </summary>
    Task<TaskResult> ExecuteAsync(Tasks.Task task, TemplateContext context, CancellationToken cancellationToken = default);
}
