# Process-Based Module Architecture

## Overview

Modules are standalone console applications that:
1. Receive input via **stdin** (JSON format)
2. Execute their logic
3. Output result via **stdout** (JSON format)
4. Exit with appropriate status code

This is exactly how Ansible works.

## Module Protocol

### Input Format (stdin)
```json
{
  "name": "nginx",
  "state": "started",
  "enabled": true
}
```

### Output Format (stdout)
```json
{
  "success": true,
  "changed": true,
  "message": "Service nginx started",
  "facts": {
    "service_status": "active"
  }
}
```

## Core Implementation

```csharp
// FulcrumLabs.Conductor.Core/Modules/ModuleExecutor.cs
namespace FulcrumLabs.Conductor.Core.Modules;

public class ModuleExecutor
{
    private readonly ModuleRegistry _registry;

    public ModuleExecutor(ModuleRegistry registry)
    {
        _registry = registry;
    }

    public async Task<ModuleResult> ExecuteAsync(string moduleName, Dictionary<string, object?> vars)
    {
        string? modulePath = _registry.GetModulePath(moduleName);
        if (modulePath == null)
            throw new ModuleNotFoundException($"Module '{moduleName}' not found");

        // Serialize vars to JSON
        string inputJson = JsonSerializer.Serialize(vars);

        // Start the module process
        var startInfo = new ProcessStartInfo
        {
            FileName = modulePath,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null)
            throw new ModuleExecutionException($"Failed to start module: {moduleName}");

        // Send input via stdin
        await process.StandardInput.WriteAsync(inputJson);
        await process.StandardInput.FlushAsync();
        process.StandardInput.Close();

        // Read output
        string stdout = await process.StandardOutput.ReadToEndAsync();
        string stderr = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        // Parse result
        try
        {
            var result = JsonSerializer.Deserialize<ModuleResult>(stdout);
            if (result == null)
                throw new ModuleExecutionException("Module returned null result");

            return result;
        }
        catch (JsonException ex)
        {
            throw new ModuleExecutionException(
                $"Module output is not valid JSON. Exit code: {process.ExitCode}\nStdout: {stdout}\nStderr: {stderr}",
                ex);
        }
    }
}

// FulcrumLabs.Conductor.Core/Modules/ModuleRegistry.cs
public class ModuleRegistry
{
    private readonly Dictionary<string, string> _modulePaths = new();

    public void RegisterModule(string name, string executablePath)
    {
        if (!File.Exists(executablePath))
            throw new FileNotFoundException($"Module executable not found: {executablePath}");

        _modulePaths[name] = executablePath;
    }

    public string? GetModulePath(string name)
    {
        _modulePaths.TryGetValue(name, out string? path);
        return path;
    }

    public void DiscoverModules(string directory)
    {
        if (!Directory.Exists(directory))
            return;

        // Look for executables matching pattern: conductor-module-*
        foreach (var file in Directory.GetFiles(directory, "conductor-module-*"))
        {
            if (!IsExecutable(file))
                continue;

            // Extract module name: conductor-module-systemd -> systemd
            string fileName = Path.GetFileNameWithoutExtension(file);
            string moduleName = fileName.Replace("conductor-module-", "");

            RegisterModule(moduleName, file);
        }
    }

    private static bool IsExecutable(string path)
    {
        // On Unix: check executable bit
        if (!OperatingSystem.IsWindows())
        {
            var unixFileInfo = new UnixFileInfo(path);
            return unixFileInfo.FileAccessPermissions.HasFlag(FileAccessPermissions.UserExecute);
        }

        // On Windows: check extension
        string ext = Path.GetExtension(path).ToLowerInvariant();
        return ext is ".exe" or ".bat" or ".cmd";
    }
}

// Result types
public class ModuleResult
{
    public bool Success { get; set; }
    public bool Changed { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object?> Facts { get; set; } = new();
}

public class ModuleNotFoundException : Exception
{
    public ModuleNotFoundException(string message) : base(message) { }
}

public class ModuleExecutionException : Exception
{
    public ModuleExecutionException(string message) : base(message) { }
    public ModuleExecutionException(string message, Exception inner) : base(message, inner) { }
}
```

## Example Module Implementation

```csharp
// conductor-module-systemd/Program.cs
using System.Text.Json;

// Read input from stdin
var inputJson = await Console.In.ReadToEndAsync();
var input = JsonSerializer.Deserialize<Dictionary<string, object?>>(inputJson);

if (input == null)
{
    Error("Invalid input JSON");
    return 1;
}

// Extract parameters
string? serviceName = input.GetValueOrDefault("name")?.ToString();
string? state = input.GetValueOrDefault("state")?.ToString();

if (string.IsNullOrEmpty(serviceName))
{
    Error("Parameter 'name' is required");
    return 1;
}

// Execute systemctl commands
bool changed = false;
string message;

try
{
    // Check current state
    var checkProcess = Process.Start(new ProcessStartInfo
    {
        FileName = "systemctl",
        Arguments = $"is-active {serviceName}",
        RedirectStandardOutput = true,
        UseShellExecute = false
    });

    await checkProcess!.WaitForExitAsync();
    bool isActive = checkProcess.ExitCode == 0;

    // Determine if action needed
    if (state == "started" && !isActive)
    {
        var startProcess = Process.Start("systemctl", $"start {serviceName}");
        await startProcess!.WaitForExitAsync();

        if (startProcess.ExitCode != 0)
        {
            Error($"Failed to start service {serviceName}");
            return 1;
        }

        changed = true;
        message = $"Service {serviceName} started";
    }
    else if (state == "stopped" && isActive)
    {
        var stopProcess = Process.Start("systemctl", $"stop {serviceName}");
        await stopProcess!.WaitForExitAsync();

        if (stopProcess.ExitCode != 0)
        {
            Error($"Failed to stop service {serviceName}");
            return 1;
        }

        changed = true;
        message = $"Service {serviceName} stopped";
    }
    else
    {
        message = $"Service {serviceName} already in desired state";
    }

    // Success - output result as JSON
    var result = new
    {
        success = true,
        changed = changed,
        message = message,
        facts = new Dictionary<string, object?>
        {
            ["service_name"] = serviceName,
            ["state"] = state
        }
    };

    Console.WriteLine(JsonSerializer.Serialize(result));
    return 0;
}
catch (Exception ex)
{
    Error($"Error executing module: {ex.Message}");
    return 1;
}

static void Error(string message)
{
    var result = new
    {
        success = false,
        changed = false,
        message = message,
        facts = new Dictionary<string, object?>()
    };
    Console.WriteLine(JsonSerializer.Serialize(result));
}
```

## Usage in CLI

```csharp
// FulcrumLabs.Conductor.Cli/Program.cs
var registry = new ModuleRegistry();

// Discover modules from standard locations
registry.DiscoverModules("/usr/local/lib/conductor/modules");
registry.DiscoverModules(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".conductor/modules"));
registry.DiscoverModules("./modules");

// Or manually register
registry.RegisterModule("systemd", "./modules/conductor-module-systemd");

// Execute a module
var executor = new ModuleExecutor(registry);

var vars = new Dictionary<string, object?>
{
    ["name"] = "nginx",
    ["state"] = "started"
};

var result = await executor.ExecuteAsync("systemd", vars);

Console.WriteLine($"Success: {result.Success}");
Console.WriteLine($"Changed: {result.Changed}");
Console.WriteLine($"Message: {result.Message}");
```

## Module Discovery

Modules can be placed in these directories (searched in order):
1. `./modules/` (relative to playbook)
2. `~/.conductor/modules/` (user modules)
3. `/usr/local/lib/conductor/modules/` (system-wide modules)

Module executables must follow naming convention: `conductor-module-<name>`

Examples:
- `conductor-module-systemd`
- `conductor-module-template`
- `conductor-module-shell`

## Benefits of This Approach

✅ **Language agnostic** - Modules can be written in Python, Rust, Go, Shell, etc.
✅ **Complete isolation** - Module crashes don't affect Conductor
✅ **Security** - Modules run as separate processes with their own permissions
✅ **Simple protocol** - Just JSON in/out
✅ **Easy testing** - Test modules standalone with `echo '{"name":"test"}' | ./conductor-module-systemd`
✅ **Ansible compatible** - Can potentially reuse Ansible modules with a wrapper
✅ **No assembly loading** - No .NET-specific complexity

## Module Development Helper

You could provide a base library for C# module authors:

```csharp
// FulcrumLabs.Conductor.ModuleSDK (NuGet package)
public abstract class ModuleBase
{
    public async Task<int> RunAsync()
    {
        try
        {
            var inputJson = await Console.In.ReadToEndAsync();
            var input = JsonSerializer.Deserialize<Dictionary<string, object?>>(inputJson);

            if (input == null)
            {
                OutputError("Invalid input JSON");
                return 1;
            }

            var result = await ExecuteAsync(input);
            OutputResult(result);
            return result.Success ? 0 : 1;
        }
        catch (Exception ex)
        {
            OutputError($"Unhandled exception: {ex.Message}");
            return 1;
        }
    }

    protected abstract Task<ModuleResult> ExecuteAsync(Dictionary<string, object?> vars);

    private void OutputResult(ModuleResult result)
    {
        Console.WriteLine(JsonSerializer.Serialize(result));
    }

    private void OutputError(string message)
    {
        OutputResult(new ModuleResult
        {
            Success = false,
            Message = message
        });
    }
}

// Usage:
class SystemdModule : ModuleBase
{
    protected override async Task<ModuleResult> ExecuteAsync(Dictionary<string, object?> vars)
    {
        // Your logic here
    }
}

// Program.cs
await new SystemdModule().RunAsync();
```
