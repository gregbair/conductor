namespace FulcrumLabs.Conductor.Modules.Common;

/// <summary>
///     The attributes that describe a module
/// </summary>
public interface IModuleAttributes
{
    /// <summary>
    ///     Gets the <see cref="Array" /> of "names" that the module responds to
    /// </summary>
    string[] RespondsTo { get; }

    /// <summary>
    ///     Gets the author of the module
    /// </summary>
    string Author { get; }

    /// <summary>
    ///     Gets the homepage of the module
    /// </summary>
    Uri Url { get; }

    /// <summary>
    ///     Gets a description of the module
    /// </summary>
    string Description { get; }
}