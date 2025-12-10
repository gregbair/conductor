namespace FulcrumLabs.Conductor.Core.Roles;

/// <summary>
/// Exception thrown when an error occurs while loading a role from the filesystem.
/// </summary>
public sealed class RoleLoadException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoleLoadException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public RoleLoadException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleLoadException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public RoleLoadException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
