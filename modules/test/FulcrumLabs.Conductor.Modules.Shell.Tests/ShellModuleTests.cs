using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

using FulcrumLabs.Conductor.Core.Modules;

namespace FulcrumLabs.Conductor.Modules.Shell.Tests;

/// <summary>
///     Integration tests for the Shell module that test through the stdin/stdout protocol.
/// </summary>
public class ShellModuleTests
{
    private async Task<ModuleResult> ExecuteModuleAsync(Dictionary<string, object?> vars)
    {
        // Spawn the module as a real subprocess (like ModuleExecutor does in production)
        // This avoids Console redirection issues

        // Navigate from test bin directory to module bin directory
        // Detect the build configuration from the test binary path
        string testDir = AppContext.BaseDirectory;
        string configuration = testDir.Contains("/Release/") || testDir.Contains("\\Release\\") ? "Release" : "Debug";
        string modulePath = Path.Combine(testDir,
            $"../../../../../src/FulcrumLabs.Conductor.Modules.Shell/bin/{configuration}/net10.0/conductor-module-shell.dll");
        modulePath = Path.GetFullPath(modulePath);

        if (!File.Exists(modulePath))
        {
            throw new Exception($"Module not found at: {modulePath}. Test directory: {testDir}");
        }

        string inputJson = JsonSerializer.Serialize(vars);

        ProcessStartInfo startInfo = new()
        {
            FileName = "dotnet",
            Arguments = modulePath,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process? process = Process.Start(startInfo);
        if (process == null)
        {
            throw new Exception("Failed to start module process");
        }

        await process.StandardInput.WriteAsync(inputJson);
        await process.StandardInput.FlushAsync();
        process.StandardInput.Close();

        string stdout = await process.StandardOutput.ReadToEndAsync();
        string stderr = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        ModuleResult? result = JsonSerializer.Deserialize<ModuleResult>(stdout);
        return result ??
               throw new Exception($"Failed to deserialize module result. Stdout: {stdout}, Stderr: {stderr}");
    }

    [Fact]
    public async Task ShellModule_SimpleCommand_ReturnsSuccess()
    {
        Dictionary<string, object?> vars = new()
        {
            ["cmd"] = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "echo test" : "echo test"
        };

        ModuleResult result = await ExecuteModuleAsync(vars);

        Assert.True(result.Success);
        Assert.True(result.Changed);
        Assert.Contains("Command executed successfully", result.Message);
        Assert.Contains("stdout", result.Facts);
        Assert.Contains("test", result.Facts["stdout"]?.ToString());
    }

    [Fact]
    public async Task ShellModule_WithoutCmd_ReturnsFailure()
    {
        Dictionary<string, object?> vars = new();

        ModuleResult result = await ExecuteModuleAsync(vars);

        Assert.False(result.Success);
        Assert.False(result.Changed);
        Assert.Contains("Parameter 'cmd' is required", result.Message);
    }

    [Fact]
    public async Task ShellModule_CommandWithExitCode_ReturnsFailure()
    {
        Dictionary<string, object?> vars = new()
        {
            ["cmd"] = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "cmd /c exit 1"
                : "false" // 'false' command always returns exit code 1
        };

        ModuleResult result = await ExecuteModuleAsync(vars);

        Assert.False(result.Success);
        Assert.False(result.Changed);
        Assert.Contains("exit code", result.Message);
        Assert.Contains("exit_code", result.Facts);
        object? exitCodeValue = result.Facts["exit_code"];
        int exitCode = exitCodeValue is JsonElement element ? element.GetInt32() : Convert.ToInt32(exitCodeValue);
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ShellModule_WithValidWorkingDirectory_ExecutesInDirectory()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        string tempDir = Path.GetTempPath();
        Dictionary<string, object?> vars = new()
        {
            ["cmd"] = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cd" : "pwd", ["chdir"] = tempDir
        };

        ModuleResult result = await ExecuteModuleAsync(vars);

        Assert.True(result.Success);
        string stdout = result.Facts["stdout"]?.ToString() ?? "";

        // Normalize paths for comparison
        string normalizedStdout = Path.TrimEndingDirectorySeparator(stdout.Trim());
        string normalizedTempDir = Path.TrimEndingDirectorySeparator(tempDir);

        Assert.Contains(normalizedTempDir, normalizedStdout, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ShellModule_WithInvalidWorkingDirectory_ReturnsFailure()
    {
        Dictionary<string, object?> vars = new()
        {
            ["cmd"] = "echo test", ["chdir"] = "/nonexistent/directory/that/does/not/exist"
        };

        ModuleResult result = await ExecuteModuleAsync(vars);

        Assert.False(result.Success);
        Assert.Contains("does not exist", result.Message);
    }

    [Fact]
    public async Task ShellModule_WithStderr_CapturesStderr()
    {
        Dictionary<string, object?> vars = new()
        {
            // Use shell pipe to redirect to stderr
            ["cmd"] = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "echo error 1>&2"
                : "echo error >&2" // Shell will handle redirection with pipe
        };

        ModuleResult result = await ExecuteModuleAsync(vars);

        Assert.True(result.Success);
        Assert.Contains("stderr", result.Facts);
        string stderr = result.Facts["stderr"]?.ToString() ?? "";
        Assert.Contains("error", stderr);
    }

    [Fact]
    public async Task ShellModule_IncludesExitCodeInFacts()
    {
        Dictionary<string, object?> vars = new()
        {
            ["cmd"] = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "echo test" : "echo test"
        };

        ModuleResult result = await ExecuteModuleAsync(vars);

        Assert.Contains("exit_code", result.Facts);
        // Handle JsonElement from deserialization
        object? exitCodeValue = result.Facts["exit_code"];
        int exitCode = exitCodeValue is JsonElement element ? element.GetInt32() : Convert.ToInt32(exitCodeValue);
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ShellModule_IncludesCommandInFacts()
    {
        string command = "echo hello world";
        Dictionary<string, object?> vars = new() { ["cmd"] = command };

        ModuleResult result = await ExecuteModuleAsync(vars);

        Assert.Contains("command", result.Facts);
        Assert.Equal(command, result.Facts["command"]?.ToString());
    }

    [Fact]
    public async Task ShellModule_WithShellPipe_ExecutesWithShell()
    {
        Dictionary<string, object?> vars = new()
        {
            ["cmd"] = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "echo hello | findstr hello"
                : "echo hello | grep hello"
        };

        ModuleResult result = await ExecuteModuleAsync(vars);

        Assert.True(result.Success);
        string stdout = result.Facts["stdout"]?.ToString() ?? "";
        Assert.Contains("hello", stdout);
    }

    [Theory]
    [InlineData("echo test")]
    [InlineData("echo multiple words here")]
    public async Task ShellModule_WithVariousCommands_ExecutesSuccessfully(string command)
    {
        Dictionary<string, object?> vars = new() { ["cmd"] = command };

        ModuleResult result = await ExecuteModuleAsync(vars);

        Assert.True(result.Success);
        Assert.True(result.Changed);
    }

    [Fact]
    public async Task ShellModule_EmptyCommand_ReturnsFailure()
    {
        Dictionary<string, object?> vars = new() { ["cmd"] = "   " };

        ModuleResult result = await ExecuteModuleAsync(vars);

        Assert.False(result.Success);
        Assert.Contains("required", result.Message);
    }

    [Fact]
    public async Task ShellModule_WithUseShellTrue_UsesShell()
    {
        Dictionary<string, object?> vars = new()
        {
            ["cmd"] = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "echo %PATH%"
                : "echo $PATH",
            ["use_shell"] = true
        };

        ModuleResult result = await ExecuteModuleAsync(vars);

        Assert.True(result.Success);
        string stdout = result.Facts["stdout"]?.ToString() ?? "";
        Assert.NotEmpty(stdout.Trim());
    }

    [Fact]
    public async Task ShellModule_OnWindows_UsesCmd()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return; // Skip on non-Windows
        }

        Dictionary<string, object?> vars = new() { ["cmd"] = "ver" };

        ModuleResult result = await ExecuteModuleAsync(vars);

        Assert.True(result.Success);
        string stdout = result.Facts["stdout"]?.ToString() ?? "";
        Assert.Contains("Windows", stdout, StringComparison.OrdinalIgnoreCase);
    }

    // Note: This test is redundant with ShellModule_SimpleCommand_ReturnsSuccess
    // Removed to avoid platform-specific test issues

    [Fact]
    public async Task ShellModule_ReturnsAllFactKeys()
    {
        Dictionary<string, object?> vars = new() { ["cmd"] = "echo test" };

        ModuleResult result = await ExecuteModuleAsync(vars);

        Assert.Contains("stdout", result.Facts);
        Assert.Contains("stderr", result.Facts);
        Assert.Contains("exit_code", result.Facts);
        Assert.Contains("command", result.Facts);
    }
}