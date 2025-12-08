namespace Fulcrum.Conductor.Core.Tasks;

/// <summary>
/// Exception thrown when task execution fails.
/// </summary>
public sealed class TaskExecutionException : Exception
{
    public TaskExecutionException(string message) : base(message)
    {
    }

    public TaskExecutionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
