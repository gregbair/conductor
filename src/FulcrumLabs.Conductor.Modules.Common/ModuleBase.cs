using System.Text.Json;

using FulcrumLabs.Conductor.Core.Modules;

namespace FulcrumLabs.Conductor.Modules.Common;

/// <summary>
///     Base class for implementing Conductor modules using the stdin/stdout JSON protocol.
/// </summary>
public abstract class ModuleBase
{
    /// <summary>
    ///     Runs the module, reading input from stdin and writing output to stdout.
    /// </summary>
    /// <returns>Exit code (0 for success, 1 for failure).</returns>
    public async Task<int> RunAsync()
    {
        try
        {
            // Read input from stdin
            string inputJson = await Console.In.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(inputJson))
            {
                OutputError("No input received from stdin");
                return 1;
            }

            Dictionary<string, object?>? input;
            try
            {
                input = JsonSerializer.Deserialize<Dictionary<string, object?>>(inputJson);
            }
            catch (JsonException ex)
            {
                OutputError($"Invalid JSON input: {ex.Message}");
                return 1;
            }

            if (input == null)
            {
                OutputError("Input JSON deserialized to null");
                return 1;
            }

            // Execute the module-specific logic
            ModuleResult result = await ExecuteAsync(input);

            // Output the result as JSON
            OutputResult(result);

            return result.Success ? 0 : 1;
        }
        catch (Exception ex)
        {
            OutputError($"Unhandled exception: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    ///     Executes the module-specific logic.
    ///     Override this method to implement your module's functionality.
    /// </summary>
    /// <param name="vars">The input variables/parameters for the module.</param>
    /// <returns>The result of the module execution.</returns>
    protected abstract Task<ModuleResult> ExecuteAsync(Dictionary<string, object?> vars);

    /// <summary>
    ///     Helper method to get a required parameter from the <paramref name="vars" /> dictionary.
    /// </summary>
    /// <param name="vars">The variables dictionary.</param>
    /// <param name="paramName">The parameter name.</param>
    /// <param name="value">The output value if found.</param>
    /// <returns>True if the parameter exists and is not null/empty, false otherwise.</returns>
    protected static bool TryGetRequiredParameter(
        Dictionary<string, object?> vars,
        string paramName,
        out string value)
    {
        if (vars.TryGetValue(paramName, out object? obj) && obj != null)
        {
            string? str = obj.ToString();
            if (!string.IsNullOrWhiteSpace(str))
            {
                value = str;
                return true;
            }
        }

        value = string.Empty;
        return false;
    }

    /// <summary>
    ///     Helper method to get an optional parameter from the vars dictionary.
    /// </summary>
    /// <param name="vars">The variables dictionary.</param>
    /// <param name="paramName">The parameter name.</param>
    /// <param name="defaultValue">The default value if not found.</param>
    /// <returns>The parameter value or the default value.</returns>
    protected static string GetOptionalParameter(
        Dictionary<string, object?> vars,
        string paramName,
        string defaultValue = "")
    {
        if (vars.TryGetValue(paramName, out object? obj) && obj != null)
        {
            string? str = obj.ToString();
            if (!string.IsNullOrWhiteSpace(str))
            {
                return str;
            }
        }

        return defaultValue;
    }

    /// <summary>
    ///     Creates a successful result.
    /// </summary>
    protected static ModuleResult Success(string message, bool changed = false,
        Dictionary<string, object?>? facts = null)
    {
        return new ModuleResult
        {
            Success = true, Changed = changed, Message = message, Facts = facts ?? new Dictionary<string, object?>()
        };
    }

    /// <summary>
    ///     Creates a failure result.
    /// </summary>
    protected static ModuleResult Failure(string message, Dictionary<string, object?>? facts = null)
    {
        return new ModuleResult
        {
            Success = false, Changed = false, Message = message, Facts = facts ?? new Dictionary<string, object?>()
        };
    }

    private static void OutputResult(ModuleResult result)
    {
        string json = JsonSerializer.Serialize(result);
        Console.WriteLine(json);
    }

    private static void OutputError(string message)
    {
        OutputResult(new ModuleResult
        {
            Success = false, Changed = false, Message = message, Facts = new Dictionary<string, object?>()
        });
    }
}