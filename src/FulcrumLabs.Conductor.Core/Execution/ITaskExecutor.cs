using FulcrumLabs.Conductor.Core.Tasks;
using FulcrumLabs.Conductor.Jinja.Rendering;

using Task = FulcrumLabs.Conductor.Core.Tasks.Task;

namespace FulcrumLabs.Conductor.Core.Execution;

/// <summary>
/// Executes a single task.
/// </summary>
public interface ITaskExecutor
{
    /// <summary>
    /// Executes a task with the given context.
    /// </summary>
    Task<TaskResult> ExecuteAsync(Task task, TemplateContext context, CancellationToken cancellationToken = default);
}
