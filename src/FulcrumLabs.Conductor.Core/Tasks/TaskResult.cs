using FulcrumLabs.Conductor.Core.Modules;

namespace FulcrumLabs.Conductor.Core.Tasks;

/// <summary>
///     Represents the result of executing a task.
///     May contain results from multiple iterations if task had a loop.
/// </summary>
public sealed class TaskResult
{
    /// <summary>
    ///     Whether the task executed successfully.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    ///     Whether the task made any changes.
    /// </summary>
    public bool Changed { get; init; }

    /// <summary>
    ///     Whether the task failed (could be true even if Success is true due to ignore_errors).
    /// </summary>
    public bool Failed { get; init; }

    /// <summary>
    ///     Whether the task was skipped due to a false conditional.
    /// </summary>
    public bool Skipped { get; init; }

    /// <summary>
    ///     Human-readable message describing the result.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    ///     For loops: results from each iteration.
    /// </summary>
    public IReadOnlyList<ModuleResult> IterationResults { get; init; } = [];

    /// <summary>
    ///     For single task execution: the module result.
    /// </summary>
    public ModuleResult? ModuleResult { get; init; }

    /// <summary>
    ///     Facts to register (if RegisterAs was specified).
    /// </summary>
    public Dictionary<string, object?> RegisteredFacts { get; init; } = new();

    /// <summary>
    ///     Creates a skipped task result.
    /// </summary>
    public static TaskResult CreateSkipped(string taskName)
    {
        return new TaskResult
        {
            Success = true,
            Changed = false,
            Failed = false,
            Skipped = true,
            Message = $"Task '{taskName}' skipped due to conditional"
        };
    }

    /// <summary>
    ///     Creates a failed task result with ignore_errors.
    /// </summary>
    public static TaskResult CreateFailedButIgnored(string taskName, string errorMessage)
    {
        return new TaskResult
        {
            Success = true, // Success is true because we're ignoring the error
            Changed = false,
            Failed = true, // But we track that it actually failed
            Skipped = false,
            Message = $"Task '{taskName}' failed (ignored): {errorMessage}"
        };
    }
}