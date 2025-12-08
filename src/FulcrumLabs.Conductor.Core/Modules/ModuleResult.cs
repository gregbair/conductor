using System.Text.Json.Serialization;

namespace FulcrumLabs.Conductor.Core.Modules;

/// <summary>
///     Represents the result of a module execution.
/// </summary>
public class ModuleResult
{
    /// <summary>
    ///     Gets or sets whether the module execution was successful.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    ///     Gets or sets whether the module made any changes to the system.
    /// </summary>
    [JsonPropertyName("changed")]
    public bool Changed { get; set; }

    /// <summary>
    ///     Gets or sets a human-readable message about the result.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets additional facts/data returned by the module.
    /// </summary>
    [JsonPropertyName("facts")]
    public Dictionary<string, object?> Facts { get; set; } = new();
}