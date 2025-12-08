using System.Runtime.InteropServices;

namespace FulcrumLabs.Conductor.Core.Modules;

/// <summary>
///     Registry for discovering and managing module executables.
/// </summary>
public class ModuleRegistry
{
    private readonly Dictionary<string, string> _modulePaths = new();

    /// <summary>
    ///     Registers a module with the given name and executable path.
    /// </summary>
    /// <param name="name">The module name (e.g., "shell", "systemd").</param>
    /// <param name="executablePath">The full path to the module executable.</param>
    /// <exception cref="FileNotFoundException">Thrown if the executable doesn't exist.</exception>
    public void RegisterModule(string name, string executablePath)
    {
        if (!File.Exists(executablePath))
        {
            throw new FileNotFoundException($"Module executable not found: {executablePath}");
        }

        _modulePaths[name] = executablePath;
    }

    /// <summary>
    ///     Gets the executable path for a module by name.
    /// </summary>
    /// <param name="name">The module name.</param>
    /// <returns>The executable path, or null if not found.</returns>
    public string? GetModulePath(string name)
    {
        _modulePaths.TryGetValue(name, out string? path);
        return path;
    }

    /// <summary>
    ///     Checks if a module is registered.
    /// </summary>
    /// <param name="name">The module name.</param>
    /// <returns>True if the module is registered, false otherwise.</returns>
    public bool HasModule(string name)
    {
        return _modulePaths.ContainsKey(name);
    }

    /// <summary>
    ///     Gets all registered module names.
    /// </summary>
    public IEnumerable<string> GetModuleNames()
    {
        return _modulePaths.Keys;
    }

    /// <summary>
    ///     Discovers modules in the specified directory.
    ///     Looks for executables matching the pattern "conductor-module-*".
    /// </summary>
    /// <param name="directory">The directory to search.</param>
    public void DiscoverModules(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return;
        }

        // Look for executables matching pattern: conductor-module-*
        string[] files = Directory.GetFiles(directory, "conductor-module-*");

        foreach (string file in files)
        {
            if (!IsExecutable(file))
            {
                continue;
            }

            // Extract module name: conductor-module-systemd -> systemd
            string fileName = Path.GetFileNameWithoutExtension(file);
            string moduleName = fileName.Replace("conductor-module-", "");

            // Only register if not already registered (first found wins)
            if (!HasModule(moduleName))
            {
                RegisterModule(moduleName, file);
            }
        }
    }

    /// <summary>
    ///     Discovers modules from standard search paths in order:
    ///     1. ./modules (relative to current directory)
    ///     2. ~/.conductor/modules (user modules)
    ///     3. /usr/local/lib/conductor/modules (system-wide modules, Unix only)
    /// </summary>
    public void DiscoverModulesFromStandardPaths()
    {
        // Local modules directory (highest priority)
        DiscoverModules("./modules");

        // User modules directory
        string userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string userModulesDir = Path.Combine(userHome, ".conductor", "modules");
        DiscoverModules(userModulesDir);

        // System-wide modules (Unix only, lowest priority)
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            DiscoverModules("/usr/local/lib/conductor/modules");
        }
    }

    private static bool IsExecutable(string path)
    {
        // On Windows: check extension
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            return ext is ".exe" or ".bat" or ".cmd";
        }

        // On Unix: check executable bit using file mode
        try
        {
            FileInfo fileInfo = new(path);
            if (!fileInfo.Exists)
            {
                return false;
            }

            // Use Unix file permissions check
            // This is a simplified check - we just try to determine if it's likely executable
            // A more robust solution would use P/Invoke to stat() or check the actual mode bits
            // For now, we'll consider files without typical non-executable extensions as potentially executable
            string ext = Path.GetExtension(path).ToLowerInvariant();
            bool hasNonExecutableExtension = ext is ".txt" or ".md" or ".json" or ".xml" or ".yml" or ".yaml";

            return !hasNonExecutableExtension;
        }
        catch
        {
            return false;
        }
    }
}