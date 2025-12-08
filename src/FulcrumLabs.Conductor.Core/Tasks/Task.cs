namespace FulcrumLabs.Conductor.Core.Tasks;

/// <summary>
/// Represents a single task to be executed.
/// Immutable after construction.
/// </summary>
public sealed class Task
{
    /// <summary>
    /// Human-readable name of the task.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Name of the module to execute.
    /// </summary>
    public string Module { get; init; } = string.Empty;

    /// <summary>
    /// Parameters to pass to the module.
    /// </summary>
    public Dictionary<string, object?> Parameters { get; init; } = new();

    /// <summary>
    /// Conditional expression. Task is skipped if this evaluates to false.
    /// </summary>
    public string? When { get; init; }

    /// <summary>
    /// Loop definition. If present, task executes multiple times.
    /// </summary>
    public LoopDefinition? Loop { get; init; }

    /// <summary>
    /// If true, task failure does not stop playbook execution.
    /// </summary>
    public bool IgnoreErrors { get; init; }

    /// <summary>
    /// Custom condition to determine if task failed.
    /// Overrides module's success status.
    /// </summary>
    public string? FailedWhen { get; init; }

    /// <summary>
    /// Custom condition to determine if task changed something.
    /// Overrides module's changed status.
    /// </summary>
    public string? ChangedWhen { get; init; }

    /// <summary>
    /// Variable name to register task result under.
    /// </summary>
    public string? RegisterAs { get; init; }
}
