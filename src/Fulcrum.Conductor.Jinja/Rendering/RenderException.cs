using Fulcrum.Conductor.Jinja.Common;

namespace Fulcrum.Conductor.Jinja.Rendering;

/// <summary>
///     Exception thrown when an error occurs during template rendering.
/// </summary>
public sealed class RenderException : JinjaException
{
    public RenderException(string message)
        : base(message)
    {
    }

    public RenderException(string message, Position position)
        : base(message, position)
    {
    }

    public RenderException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public RenderException(string message, Position position, Exception innerException)
        : base(message, position, innerException)
    {
    }
}