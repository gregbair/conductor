using Fulcrum.Conductor.Core.Modules;

// Example of using the process-based module system

// Step 1: Create a module registry and discover modules
var registry = new ModuleRegistry();

// Discover modules from standard paths
// (./modules, ~/.conductor/modules, /usr/local/lib/conductor/modules)
registry.DiscoverModulesFromStandardPaths();

// Or manually register a specific module
var shellModulePath = "./modules/src/Fulcrum.Conductor.Modules.Shell/bin/Debug/net10.0/conductor-module-shell";
if (File.Exists(shellModulePath))
{
    registry.RegisterModule("shell", shellModulePath);
}

// Step 2: Create a module executor
var executor = new ModuleExecutor(registry);

// Step 3: Execute a module
try
{
    var vars = new Dictionary<string, object?>
    {
        ["cmd"] = "echo 'Hello from Conductor!'",
        ["chdir"] = Directory.GetCurrentDirectory()
    };

    Console.WriteLine("Executing shell module...");
    var result = await executor.ExecuteAsync("shell", vars);

    Console.WriteLine($"Success: {result.Success}");
    Console.WriteLine($"Changed: {result.Changed}");
    Console.WriteLine($"Message: {result.Message}");

    if (result.Facts.TryGetValue("stdout", out var stdout))
    {
        Console.WriteLine($"Output: {stdout}");
    }

    if (result.Facts.TryGetValue("exit_code", out var exitCode))
    {
        Console.WriteLine($"Exit Code: {exitCode}");
    }
}
catch (ModuleNotFoundException ex)
{
    Console.WriteLine($"Module not found: {ex.Message}");
}
catch (ModuleExecutionException ex)
{
    Console.WriteLine($"Module execution failed: {ex.Message}");
}

// Example: List all registered modules
Console.WriteLine("\nRegistered modules:");
foreach (var moduleName in registry.GetModuleNames())
{
    Console.WriteLine($"  - {moduleName}");
}
