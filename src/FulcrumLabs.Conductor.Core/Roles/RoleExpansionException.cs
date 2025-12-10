namespace FulcrumLabs.Conductor.Core.Roles;

/// <summary>
/// Exception thrown when an error occurs while expanding a role reference.
/// </summary>
public sealed class RoleExpansionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoleExpansionException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public RoleExpansionException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleExpansionException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public RoleExpansionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
