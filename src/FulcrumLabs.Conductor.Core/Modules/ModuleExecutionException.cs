namespace FulcrumLabs.Conductor.Core.Modules;

/// <summary>
///     Exception thrown when a module execution fails.
/// </summary>
public class ModuleExecutionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleExecutionException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ModuleExecutionException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleExecutionException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ModuleExecutionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}