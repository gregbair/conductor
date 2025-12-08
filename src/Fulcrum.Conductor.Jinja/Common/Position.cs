namespace Fulcrum.Conductor.Jinja.Common;

/// <summary>
///     Represents a position in the source template for error reporting.
/// </summary>
public record Position(int Line, int Column, int Index);