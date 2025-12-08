using FulcrumLabs.Conductor.Core.Playbooks;

namespace FulcrumLabs.Conductor.Core.Yaml;

/// <summary>
/// Loads playbooks from YAML files.
/// </summary>
public interface IPlaybookLoader
{
    /// <summary>
    /// Loads a playbook from a YAML file.
    /// </summary>
    Playbook LoadFromFile(string filePath);

    /// <summary>
    /// Loads a playbook from a YAML string.
    /// </summary>
    Playbook LoadFromString(string yamlContent);
}
