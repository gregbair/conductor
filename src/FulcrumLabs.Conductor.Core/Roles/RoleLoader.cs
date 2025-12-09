using FulcrumLabs.Conductor.Core.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Task = FulcrumLabs.Conductor.Core.Tasks.Task;

namespace FulcrumLabs.Conductor.Core.Roles;

/// <summary>
/// Loads roles from the filesystem following Ansible directory structure.
/// </summary>
public sealed class RoleLoader : IRoleLoader
{
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    /// <inheritdoc />
    public Role LoadRole(string roleName, string rolesBasePath = "./roles")
    {
        try
        {
            string rolePath = Path.Combine(rolesBasePath, roleName);

            if (!Directory.Exists(rolePath))
            {
                throw new RoleLoadException($"Role directory not found: {rolePath}");
            }

            // Load tasks (required)
            List<Task> tasks = LoadRoleTasks(rolePath, roleName);

            // Load defaults (optional)
            Dictionary<string, object?> defaults = LoadRoleVarsFile(rolePath, "defaults");

            // Load vars (optional)
            Dictionary<string, object?> vars = LoadRoleVarsFile(rolePath, "vars");

            return new Role
            {
                Name = roleName,
                Tasks = tasks,
                Defaults = defaults,
                Vars = vars,
                Parameters = new Dictionary<string, object?>()
            };
        }
        catch (RoleLoadException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new RoleLoadException($"Failed to load role '{roleName}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Loads tasks from a role's tasks/main.yml file.
    /// </summary>
    /// <param name="rolePath">The path to the role directory.</param>
    /// <param name="roleName">The name of the role (for error messages).</param>
    /// <returns>The list of tasks defined in the role.</returns>
    /// <exception cref="RoleLoadException">Thrown when tasks cannot be loaded.</exception>
    private List<Task> LoadRoleTasks(string rolePath, string roleName)
    {
        string tasksFile = Path.Combine(rolePath, "tasks", "main.yml");

        if (!File.Exists(tasksFile))
        {
            throw new RoleLoadException($"Role '{roleName}' is missing required tasks/main.yml file");
        }

        try
        {
            string yamlContent = File.ReadAllText(tasksFile);
            List<object> taskDataList = _deserializer.Deserialize<List<object>>(yamlContent);

            if (taskDataList == null || taskDataList.Count == 0)
            {
                return new List<Task>();
            }

            List<Task> tasks = new();
            foreach (object taskObj in taskDataList)
            {
                if (taskObj is Dictionary<object, object> taskDict)
                {
                    tasks.Add(BuildTask(taskDict, roleName));
                }
            }

            return tasks;
        }
        catch (Exception ex)
        {
            throw new RoleLoadException($"Failed to load tasks for role '{roleName}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Loads variables from a role's defaults/main.yml or vars/main.yml file.
    /// </summary>
    /// <param name="rolePath">The path to the role directory.</param>
    /// <param name="subdirectory">The subdirectory name ("defaults" or "vars").</param>
    /// <returns>The variables defined in the file, or an empty dictionary if the file doesn't exist.</returns>
    /// <exception cref="RoleLoadException">Thrown when the vars file exists but cannot be parsed.</exception>
    private Dictionary<string, object?> LoadRoleVarsFile(string rolePath, string subdirectory)
    {
        string varsFile = Path.Combine(rolePath, subdirectory, "main.yml");

        if (!File.Exists(varsFile))
        {
            return new Dictionary<string, object?>();
        }

        try
        {
            string yamlContent = File.ReadAllText(varsFile);
            Dictionary<object, object>? varsData = _deserializer.Deserialize<Dictionary<object, object>>(yamlContent);

            if (varsData == null)
            {
                return new Dictionary<string, object?>();
            }

            Dictionary<string, object?> vars = new();
            foreach ((object key, object value) in varsData)
            {
                vars[key.ToString()!] = ConvertYamlValue(value);
            }

            return vars;
        }
        catch (Exception ex)
        {
            throw new RoleLoadException($"Failed to load {subdirectory} for role: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Builds a Task object from YAML task data.
    /// </summary>
    /// <param name="taskData">The deserialized task data.</param>
    /// <param name="roleName">The name of the role (for error messages).</param>
    /// <returns>The constructed Task object.</returns>
    /// <exception cref="RoleLoadException">Thrown when the task cannot be built.</exception>
    private Task BuildTask(Dictionary<object, object> taskData, string roleName)
    {
        // Convert keys to strings for easier access
        Dictionary<string, object> taskDict = taskData.ToDictionary(
            kvp => kvp.Key.ToString()!,
            kvp => kvp.Value
        );

        // Extract task name
        string name = GetStringValue(taskDict, "name", "Unnamed task") ?? "Unnamed task";

        // Extract control flow properties
        string? when = GetStringValue(taskDict, "when", null);
        bool ignoreErrors = GetBoolValue(taskDict, "ignore_errors", false);
        string? failedWhen = GetStringValue(taskDict, "failed_when", null);
        string? changedWhen = GetStringValue(taskDict, "changed_when", null);
        string? register = GetStringValue(taskDict, "register", null);

        // Extract loop
        LoopDefinition? loop = null;
        if (taskDict.TryGetValue("loop", out object? loopObj))
        {
            loop = new SimpleLoopDefinition { Items = loopObj };
        }
        else if (taskDict.TryGetValue("with_items", out object? withItemsObj))
        {
            loop = new WithItemsLoopDefinition { Items = withItemsObj };
        }

        // Find the module name and parameters
        string? moduleName = null;
        Dictionary<string, object?> parameters = new();

        // Check for common module names
        string[] knownModules = ["shell", "debug", "command", "copy", "file", "template", "apt", "yum", "service"];
        foreach (string knownModule in knownModules)
        {
            if (taskDict.TryGetValue(knownModule, out object? moduleParams))
            {
                moduleName = knownModule;
                parameters = ParseModuleParameters(moduleParams, moduleName);
                break;
            }
        }

        // If no known module found, check for generic 'module' key
        if (moduleName == null && taskDict.TryGetValue("module", out object? genericModule))
        {
            moduleName = genericModule.ToString();
        }

        if (string.IsNullOrEmpty(moduleName))
        {
            throw new RoleLoadException($"Task '{name}' in role '{roleName}' does not specify a module");
        }

        return new Task
        {
            Name = name,
            Module = moduleName!,  // Safe: we've already checked it's not null above
            Parameters = parameters,
            When = when,
            Loop = loop,
            IgnoreErrors = ignoreErrors,
            FailedWhen = failedWhen,
            ChangedWhen = changedWhen,
            RegisterAs = register
        };
    }

    /// <summary>
    /// Parses module parameters from various YAML formats.
    /// </summary>
    /// <param name="moduleParams">The module parameters from YAML.</param>
    /// <param name="moduleName">The name of the module.</param>
    /// <returns>A dictionary of parsed parameters.</returns>
    private Dictionary<string, object?> ParseModuleParameters(object? moduleParams, string moduleName)
    {
        Dictionary<string, object?> parameters = new();

        switch (moduleParams)
        {
            // String parameter (e.g., shell: "echo hello")
            case string strParam:
                {
                    // Map string parameter to module-specific key
                    string paramKey = GetDefaultParameterKey(moduleName);
                    parameters[paramKey] = strParam;
                    break;
                }
            // Dictionary parameters (e.g., debug: { msg: "hello" })
            case Dictionary<object, object> dictParam:
                {
                    foreach ((object key, object value) in dictParam)
                    {
                        parameters[key.ToString()!] = value;
                    }
                    break;
                }
        }

        return parameters;
    }

    /// <summary>
    /// Gets the default parameter key for a module when parameters are provided as a string.
    /// </summary>
    /// <param name="moduleName">The name of the module.</param>
    /// <returns>The default parameter key.</returns>
    private static string GetDefaultParameterKey(string moduleName)
    {
        return moduleName switch
        {
            "shell" => "cmd",
            "command" => "cmd",
            "debug" => "msg",
            _ => "_raw"
        };
    }

    /// <summary>
    /// Gets a string value from a dictionary, with a default fallback.
    /// </summary>
    /// <param name="dict">The dictionary to query.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="defaultValue">The default value if key is not found.</param>
    /// <returns>The string value or default.</returns>
    private static string? GetStringValue(Dictionary<string, object> dict, string key, string? defaultValue)
    {
        if (dict.TryGetValue(key, out object? value) && value != null)
        {
            return value.ToString();
        }

        return defaultValue;
    }

    /// <summary>
    /// Gets a boolean value from a dictionary, with a default fallback.
    /// </summary>
    /// <param name="dict">The dictionary to query.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="defaultValue">The default value if key is not found.</param>
    /// <returns>The boolean value or default.</returns>
    private static bool GetBoolValue(Dictionary<string, object> dict, string key, bool defaultValue)
    {
        if (dict.TryGetValue(key, out object? value))
        {
            if (value is bool boolValue)
            {
                return boolValue;
            }

            if (value != null && bool.TryParse(value.ToString(), out bool parsedValue))
            {
                return parsedValue;
            }
        }

        return defaultValue;
    }

    /// <summary>
    /// Converts YAML scalar values from strings to their proper types.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted value with proper type.</returns>
    private static object? ConvertYamlValue(object? value)
    {
        if (value == null)
        {
            return null;
        }

        // If it's already a non-string type, return it as-is
        if (value is not string stringValue)
        {
            return value;
        }

        // Try to parse as boolean
        if (bool.TryParse(stringValue, out bool boolValue))
        {
            return boolValue;
        }

        // Try to parse as int
        if (int.TryParse(stringValue, out int intValue))
        {
            return intValue;
        }

        // Try to parse as double
        if (double.TryParse(stringValue, out double doubleValue))
        {
            return doubleValue;
        }

        // Return as string
        return stringValue;
    }
}
