# Conductor Module System

## Overview

Conductor uses a process-based module architecture where modules are standalone console applications that communicate via JSON over stdin/stdout. This design provides:

- **Language agnostic** - Modules can be written in any language
- **Process isolation** - Module crashes don't affect Conductor
- **Simple protocol** - Just JSON in/out
- **Easy testing** - Test modules standalone
- **Security** - Each module runs as a separate process

## Architecture

```
┌─────────────┐
│  Conductor  │
│    Core     │
└──────┬──────┘
       │
       │ JSON via stdin/stdout
       │
┌──────▼──────────┐
│  Module Process │
│ (conductor-     │
│  module-shell)  │
└─────────────────┘
```

## Module Protocol

### Input (stdin)
Modules receive JSON input via stdin:
```json
{
  "cmd": "echo Hello",
  "chdir": "/tmp"
}
```

### Output (stdout)
Modules output JSON results via stdout:
```json
{
  "success": true,
  "changed": true,
  "message": "Command executed successfully",
  "facts": {
    "stdout": "Hello\n",
    "stderr": "",
    "exit_code": 0
  }
}
```

## Using the Module System

### 1. Register Modules

```csharp
using FulcrumLabs.Conductor.Core.Modules;

var registry = new ModuleRegistry();

// Auto-discover from standard paths
registry.DiscoverModulesFromStandardPaths();

// Or manually register
registry.RegisterModule("shell", "/path/to/conductor-module-shell");
```

### 2. Execute Modules

```csharp
var executor = new ModuleExecutor(registry);

var vars = new Dictionary<string, object?>
{
    ["cmd"] = "echo Hello",
    ["chdir"] = "/tmp"
};

var result = await executor.ExecuteAsync("shell", vars);

Console.WriteLine($"Success: {result.Success}");
Console.WriteLine($"Changed: {result.Changed}");
Console.WriteLine($"Message: {result.Message}");
```

## Creating a Module

### Option 1: Using ModuleBase (C#)

```csharp
using FulcrumLabs.Conductor.Core.Modules;
using FulcrumLabs.Conductor.Modules.Common;

public class MyModule : ModuleBase
{
    protected override async Task<ModuleResult> ExecuteAsync(Dictionary<string, object?> vars)
    {
        // Get required parameter
        if (!TryGetRequiredParameter(vars, "name", out string name))
        {
            return Failure("Parameter 'name' is required");
        }

        // Get optional parameter
        string greeting = GetOptionalParameter(vars, "greeting", "Hello");

        // Do your work here
        await DoSomethingAsync(name);

        // Return success
        return Success(
            message: $"{greeting}, {name}!",
            changed: true,
            facts: new Dictionary<string, object?>
            {
                ["name"] = name,
                ["greeting"] = greeting
            });
    }
}

// Program.cs
return await new MyModule().RunAsync();
```

### Option 2: Any Language

Modules can be written in any language. Example in Python:

```python
#!/usr/bin/env python3
import sys
import json

# Read input from stdin
input_data = json.load(sys.stdin)

# Get parameters
name = input_data.get('name')
if not name:
    result = {
        'success': False,
        'changed': False,
        'message': "Parameter 'name' is required",
        'facts': {}
    }
    print(json.dumps(result))
    sys.exit(1)

# Do work here
# ...

# Output result
result = {
    'success': True,
    'changed': True,
    'message': f'Processed {name}',
    'facts': {'name': name}
}
print(json.dumps(result))
sys.exit(0)
```

## Module Discovery

Modules are discovered from these directories (in order):

1. `./modules/` - Local to playbook (highest priority)
2. `~/.conductor/modules/` - User modules
3. `/usr/local/lib/conductor/modules/` - System-wide (Unix only)

Module executables must be named: `conductor-module-<name>`

Examples:
- `conductor-module-shell`
- `conductor-module-systemd`
- `conductor-module-template`

## Testing Modules

Test modules standalone using echo:

```bash
# Test the shell module
echo '{"cmd": "ls -la"}' | ./conductor-module-shell

# Expected output:
# {"success":true,"changed":true,"message":"Command executed successfully","facts":{...}}
```

## Built-in Modules

### shell
Executes shell commands.

Parameters:
- `cmd` (required): The command to execute
- `chdir` (optional): Working directory
- `use_shell` (optional): Force using shell for execution

Example:
```csharp
var result = await executor.ExecuteAsync("shell", new Dictionary<string, object?>
{
    ["cmd"] = "ls -la /tmp",
    ["chdir"] = "/home/user"
});
```

## Project Structure

```
src/
  FulcrumLabs.Conductor.Core/
    Modules/
      ModuleRegistry.cs      - Discovers and manages modules
      ModuleExecutor.cs      - Executes modules via process spawning
      ModuleResult.cs        - Result type
      ModuleNotFoundException.cs
      ModuleExecutionException.cs

  FulcrumLabs.Conductor.Modules.Common/
    ModuleBase.cs           - Base class for C# modules
    IModuleAttributes.cs    - Module metadata interface

modules/
  src/
    FulcrumLabs.Conductor.Modules.Shell/
      ShellModule.cs        - Shell command module implementation
      Program.cs            - Entry point
```

## Error Handling

```csharp
try
{
    var result = await executor.ExecuteAsync("shell", vars);

    if (!result.Success)
    {
        Console.WriteLine($"Module failed: {result.Message}");
    }
}
catch (ModuleNotFoundException ex)
{
    Console.WriteLine($"Module not found: {ex.Message}");
}
catch (ModuleExecutionException ex)
{
    Console.WriteLine($"Execution failed: {ex.Message}");
}
```

## Module Timeouts

Default timeout is 5 minutes. Override per execution:

```csharp
var executor = new ModuleExecutor(registry, defaultTimeout: TimeSpan.FromMinutes(10));

// Or per execution
var result = await executor.ExecuteAsync(
    "shell",
    vars,
    timeout: TimeSpan.FromSeconds(30));
```

## Best Practices

1. **Stateless modules** - All state should come through input vars
2. **Clear error messages** - Help users understand what went wrong
3. **Structured facts** - Return useful data in the `facts` dictionary
4. **Idempotency** - Set `changed: false` when no changes were made
5. **Validation** - Validate required parameters early
6. **Timeout handling** - Design modules to complete within reasonable time
