using Fulcrum.Conductor.Core.Playbooks;

namespace Fulcrum.Conductor.Core.Yaml;

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
