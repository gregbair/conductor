namespace FulcrumLabs.Conductor.Core.Modules;

/// <summary>
///     Exception thrown when a requested module cannot be found.
/// </summary>
public class ModuleNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleNotFoundException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ModuleNotFoundException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleNotFoundException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ModuleNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}