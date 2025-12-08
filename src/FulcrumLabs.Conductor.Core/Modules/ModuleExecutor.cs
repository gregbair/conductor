using System.Diagnostics;
using System.Text.Json;

namespace FulcrumLabs.Conductor.Core.Modules;

/// <summary>
///     Executes modules as external processes using the stdin/stdout JSON protocol.
/// </summary>
public class ModuleExecutor
{
    private readonly TimeSpan _defaultTimeout;
    private readonly ModuleRegistry _registry;

    /// <summary>
    ///     Creates a new module executor.
    /// </summary>
    /// <param name="registry">The module registry to use for looking up module paths.</param>
    /// <param name="defaultTimeout">Default timeout for module execution. Defaults to 5 minutes.</param>
    public ModuleExecutor(ModuleRegistry registry, TimeSpan? defaultTimeout = null)
    {
        _registry = registry;
        _defaultTimeout = defaultTimeout ?? TimeSpan.FromMinutes(5);
    }

    /// <summary>
    ///     Executes a module with the given parameters.
    /// </summary>
    /// <param name="moduleName">The name of the module to execute.</param>
    /// <param name="vars">The variables/parameters to pass to the module.</param>
    /// <param name="timeout">Optional timeout override for this execution.</param>
    /// <returns>The result of the module execution.</returns>
    /// <exception cref="ModuleNotFoundException">Thrown if the module is not found in the registry.</exception>
    /// <exception cref="ModuleExecutionException">Thrown if the module execution fails.</exception>
    public async Task<ModuleResult> ExecuteAsync(
        string moduleName,
        Dictionary<string, object?> vars,
        TimeSpan? timeout = null)
    {
        string? modulePath = _registry.GetModulePath(moduleName);
        if (modulePath == null)
        {
            throw new ModuleNotFoundException($"Module '{moduleName}' not found in registry");
        }

        TimeSpan effectiveTimeout = timeout ?? _defaultTimeout;

        // Serialize vars to JSON
        string inputJson = JsonSerializer.Serialize(vars);

        // Start the module process
        ProcessStartInfo startInfo = new()
        {
            FileName = modulePath,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process? process = Process.Start(startInfo);
        if (process == null)
        {
            throw new ModuleExecutionException($"Failed to start module: {moduleName}");
        }

        try
        {
            // Send input via stdin
            await process.StandardInput.WriteAsync(inputJson);
            await process.StandardInput.FlushAsync();
            process.StandardInput.Close();

            // Read output with timeout
            Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
            Task<string> errorTask = process.StandardError.ReadToEndAsync();
            Task waitTask = process.WaitForExitAsync();

            Task completedTask = await Task.WhenAny(waitTask, Task.Delay(effectiveTimeout));

            if (completedTask != waitTask)
            {
                // Timeout occurred
                try
                {
                    process.Kill(true);
                }
                catch
                {
                    // Ignore errors during kill
                }

                throw new ModuleExecutionException(
                    $"Module '{moduleName}' execution timed out after {effectiveTimeout.TotalSeconds} seconds");
            }

            // Process completed, get the output
            string stdout = await outputTask;
            string stderr = await errorTask;

            // Parse result
            try
            {
                ModuleResult? result = JsonSerializer.Deserialize<ModuleResult>(stdout);
                if (result == null)
                {
                    throw new ModuleExecutionException(
                        $"Module '{moduleName}' returned null result.\n" +
                        $"Exit code: {process.ExitCode}\n" +
                        $"Stdout: {stdout}\n" +
                        $"Stderr: {stderr}");
                }

                return result;
            }
            catch (JsonException ex)
            {
                throw new ModuleExecutionException(
                    $"Module '{moduleName}' output is not valid JSON.\n" +
                    $"Exit code: {process.ExitCode}\n" +
                    $"Stdout: {stdout}\n" +
                    $"Stderr: {stderr}",
                    ex);
            }
        }
        catch (Exception ex) when (ex is not ModuleExecutionException and not ModuleNotFoundException)
        {
            throw new ModuleExecutionException(
                $"Unexpected error executing module '{moduleName}': {ex.Message}",
                ex);
        }
    }
}