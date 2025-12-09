using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using FulcrumLabs.Conductor.Core.Modules;
using FulcrumLabs.Conductor.Modules.Common;

namespace FulcrumLabs.Conductor.Modules.Shell;

/// <summary>
///     Executes shell commands, similar to Ansible's shell/command module.
/// </summary>
public class ShellModule : ModuleBase
{
    /// <inheritdoc />
    protected override async Task<ModuleResult> ExecuteAsync(Dictionary<string, object?> vars)
    {
        // Get the command to execute
        if (!TryGetRequiredParameter(vars, "cmd", out string command))
        {
            return Failure("Parameter 'cmd' is required");
        }

        // Get optional parameters
        string workingDir = GetOptionalParameter(vars, "chdir", Directory.GetCurrentDirectory());
        bool useShell = vars.TryGetValue("use_shell", out object? useShellObj) &&
                        useShellObj is bool useShellBool && useShellBool;

        // Validate working directory
        if (!Directory.Exists(workingDir))
        {
            return Failure($"Working directory does not exist: {workingDir}");
        }

        try
        {
            // Prepare process start info
            ProcessStartInfo startInfo = new()
            {
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Configure based on OS and shell preference
            if (useShell || command.Contains('|') || command.Contains('>') || command.Contains('<'))
            {
                // Use shell for commands with pipes/redirects
                if (OperatingSystem.IsWindows())
                {
                    startInfo.FileName = "cmd.exe";
                    startInfo.Arguments = $"/c {command}";
                }
                else
                {
                    startInfo.FileName = "/bin/sh";
                    startInfo.Arguments = $"-c \"{command.Replace("\"", "\\\"")}\"";
                }
            }
            else
            {
                // Parse command and arguments
                string[] parts = command.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                {
                    return Failure("Empty command");
                }

                startInfo.FileName = parts[0];
                if (parts.Length > 1)
                {
                    startInfo.Arguments = parts[1];
                }
            }

            // Execute the command
            using Process? process = Process.Start(startInfo);
            if (process == null)
            {
                return Failure($"Failed to start process: {command}");
            }

            // Read output
            Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
            Task<string> stderrTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            string stdout = await stdoutTask;
            string stderr = await stderrTask;
            int exitCode = process.ExitCode;

            // Build result
            Dictionary<string, object?> facts = new()
            {
                ["stdout"] = stdout, ["stderr"] = stderr, ["exit_code"] = exitCode, ["command"] = command
            };

            if (exitCode == 0)
            {
                return Success(
                    "Command executed successfully",
                    true,
                    facts);
            }

            return Failure(
                $"Command failed with exit code {exitCode}: {stderr}",
                facts);
        }
        catch (Exception ex)
        {
            return Failure($"Error executing command: {ex.Message}");
        }
    }
}