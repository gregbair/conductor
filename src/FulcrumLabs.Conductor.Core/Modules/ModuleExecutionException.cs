namespace FulcrumLabs.Conductor.Core.Modules;

/// <summary>
///     Exception thrown when a module execution fails.
/// </summary>
public class ModuleExecutionException : Exception
{
    public ModuleExecutionException(string message) : base(message)
    {
    }

    public ModuleExecutionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}