namespace FulcrumLabs.Conductor.Core.Modules;

/// <summary>
///     Exception thrown when a requested module cannot be found.
/// </summary>
public class ModuleNotFoundException : Exception
{
    public ModuleNotFoundException(string message) : base(message)
    {
    }

    public ModuleNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}