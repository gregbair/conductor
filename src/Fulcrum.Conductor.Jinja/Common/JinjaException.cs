namespace Fulcrum.Conductor.Jinja.Common;

/// <summary>
///     Base exception class for all Jinja2 template engine errors.
/// </summary>
public class JinjaException : Exception
{
    public JinjaException(string message)
        : base(message)
    {
    }

    public JinjaException(string message, Position position)
        : base($"{message} at {position}")
    {
        Position = position;
    }

    public JinjaException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public JinjaException(string message, Position position, Exception innerException)
        : base($"{message} at {position}", innerException)
    {
        Position = position;
    }

    /// <summary>
    ///     Gets the position in the template where the error occurred.
    /// </summary>
    public Position? Position { get; }
}