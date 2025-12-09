using FulcrumLabs.Conductor.Jinja.Common;

namespace FulcrumLabs.Conductor.Jinja.Rendering;

/// <summary>
///     Exception thrown when an error occurs during template rendering.
/// </summary>
public sealed class RenderException : JinjaException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RenderException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public RenderException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RenderException"/> class with a specified error message and position.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="position">The position in the template where the error occurred.</param>
    public RenderException(string message, Position position)
        : base(message, position)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RenderException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public RenderException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RenderException"/> class with a specified error message, position, and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="position">The position in the template where the error occurred.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public RenderException(string message, Position position, Exception innerException)
        : base(message, position, innerException)
    {
    }
}