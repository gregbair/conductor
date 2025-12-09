namespace FulcrumLabs.Conductor.Core.Tasks;

/// <summary>
/// Exception thrown when task execution fails.
/// </summary>
public sealed class TaskExecutionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TaskExecutionException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public TaskExecutionException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskExecutionException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public TaskExecutionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
