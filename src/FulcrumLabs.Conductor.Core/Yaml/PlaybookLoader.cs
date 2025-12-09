using FulcrumLabs.Conductor.Core.Playbooks;
using FulcrumLabs.Conductor.Core.Tasks;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

using Task = FulcrumLabs.Conductor.Core.Tasks.Task;
using Tasks_Task = FulcrumLabs.Conductor.Core.Tasks.Task;

namespace FulcrumLabs.Conductor.Core.Yaml;

/// <summary>
///     Loads playbooks from YAML using YamlDotNet.
/// </summary>
public sealed class PlaybookLoader : IPlaybookLoader
{
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    /// <inheritdoc />
    public Playbook LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new PlaybookLoadException($"Playbook file not found: {filePath}");
        }

        string yamlContent = File.ReadAllText(filePath);
        return LoadFromString(yamlContent);
    }

    /// <inheritdoc />
    public Playbook LoadFromString(string yamlContent)
    {
        try
        {
            // Deserialize to list of play data (Ansible playbooks are lists of plays)
            List<Dictionary<string, object>> playDataList =
                _deserializer.Deserialize<List<Dictionary<string, object>>>(yamlContent);

            if (playDataList == null || playDataList.Count == 0)
            {
                throw new PlaybookLoadException("Playbook is empty or invalid");
            }

            // Build plays
            List<Play> plays = [];
            foreach (Dictionary<string, object> playData in playDataList)
            {
                plays.Add(BuildPlay(playData));
            }

            return new Playbook { Plays = plays };
        }
        catch (PlaybookLoadException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new PlaybookLoadException($"Failed to load playbook: {ex.Message}", ex);
        }
    }

    private Play BuildPlay(Dictionary<string, object> playData)
    {
        // Extract play name
        string name = GetStringValue(playData, "name", "Unnamed play");

        // Extract vars
        Dictionary<string, object?> vars = [];
        if (playData.TryGetValue("vars", out object? varsObj) && varsObj is Dictionary<object, object> varsDict)
        {
            foreach ((object key, object value) in varsDict)
            {
                vars[key.ToString()!] = ConvertYamlValue(value);
            }
        }

        // Extract tasks
        List<Tasks_Task> tasks = [];
        if (playData.TryGetValue("tasks", out object? tasksObj) && tasksObj is List<object> tasksList)
        {
            foreach (object taskObj in tasksList)
            {
                if (taskObj is Dictionary<object, object> taskDict)
                {
                    tasks.Add(BuildTask(taskDict));
                }
            }
        }

        return new Play { Name = name, Vars = vars, Tasks = tasks };
    }

    private Tasks_Task BuildTask(Dictionary<object, object> taskData)
    {
        // Convert keys to strings for easier access
        Dictionary<string, object> taskDict = taskData.ToDictionary(
            kvp => kvp.Key.ToString()!,
            kvp => kvp.Value
        );

        // Extract task name
        string name = GetStringValue(taskDict, "name", "Unnamed task");

        // Extract control flow properties
        string when = GetStringValue(taskDict, "when", null);
        bool ignoreErrors = GetBoolValue(taskDict, "ignore_errors", false);
        string failedWhen = GetStringValue(taskDict, "failed_when", null);
        string changedWhen = GetStringValue(taskDict, "changed_when", null);
        string register = GetStringValue(taskDict, "register", null);

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
        Dictionary<string, object?> parameters = [];

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
            throw new PlaybookLoadException($"Task '{name}' does not specify a module");
        }

        return new Tasks_Task
        {
            Name = name,
            Module = moduleName,
            Parameters = parameters,
            When = when,
            Loop = loop,
            IgnoreErrors = ignoreErrors,
            FailedWhen = failedWhen,
            ChangedWhen = changedWhen,
            RegisterAs = register
        };
    }

    private Dictionary<string, object?> ParseModuleParameters(object? moduleParams, string moduleName)
    {
        Dictionary<string, object?> parameters = [];

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

    private static string GetDefaultParameterKey(string moduleName)
    {
        // Map module names to their default parameter keys
        return moduleName switch
        {
            "shell" => "cmd",
            "command" => "cmd",
            "debug" => "msg",
            _ => "_raw"
        };
    }

    private static string GetStringValue(Dictionary<string, object> dict, string key, string? defaultValue)
    {
        if (dict.TryGetValue(key, out object? value))
        {
            return value.ToString()!;
        }

        return defaultValue ?? string.Empty;
    }

    private static bool GetBoolValue(Dictionary<string, object> dict, string key, bool defaultValue)
    {
        if (dict.TryGetValue(key, out object? value))
        {
            if (value is bool boolValue)
            {
                return boolValue;
            }

            if (bool.TryParse(value.ToString(), out bool parsedValue))
            {
                return parsedValue;
            }
        }

        return defaultValue;
    }

    /// <summary>
    ///     Converts YAML scalar values from strings to their proper types.
    ///     YamlDotNet deserializes to Dictionary&lt;object, object&gt; which treats scalars as strings.
    /// </summary>
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

/// <summary>
///     Exception thrown when playbook loading fails.
/// </summary>
public sealed class PlaybookLoadException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybookLoadException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public PlaybookLoadException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybookLoadException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public PlaybookLoadException(string message, Exception innerException) : base(message, innerException)
    {
    }
}