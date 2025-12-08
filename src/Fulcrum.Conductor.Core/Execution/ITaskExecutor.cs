using Fulcrum.Conductor.Core.Tasks;
using Fulcrum.Conductor.Jinja.Rendering;

namespace Fulcrum.Conductor.Core.Execution;

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
