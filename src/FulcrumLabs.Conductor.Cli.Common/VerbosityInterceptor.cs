using System;
using System.IO;

using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

using Spectre.Console.Cli;

namespace FulcrumLabs.Conductor.Cli.Common;

/// <summary>
///     Sets logging verbosity based on -v through -vvvv passed in to the command line
/// </summary>
public class VerbosityInterceptor : ICommandInterceptor
{
    /// <inheritdoc />
    public void Intercept(CommandContext context, CommandSettings settings)
    {
        if (settings is not BaseCommandSettings baseSettings)
        {
            return;
        }

        LogEventLevel level = baseSettings.GetLogLevel();

        // dispose old logger
        (Log.Logger as IDisposable)?.Dispose();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(level)
            .WriteTo.Console(
                LogEventLevel.Warning,
                "{Message:lj}{NewLine}{Exception}",
                theme: AnsiConsoleTheme.Code)
            .WriteTo.File(
                Path.Combine(GetLogDirectory(), "conductor.log"),
                level,
                fileSizeLimitBytes: 1000000,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 3)
            .CreateLogger();
    }

    private static string GetLogDirectory()
    {
        // We may be running under sudo. We have to compensate for that.
        string logDir;

        if (Environment.UserName == "root" && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SUDO_USER")))
        {
            string sudoUser = Environment.GetEnvironmentVariable("SUDO_USER")!;
            string sudoHome = Environment.GetEnvironmentVariable("SUDO_HOME") ?? $"/home/{sudoUser}";

            logDir = Path.Combine(sudoHome, ".local", "share", "fulcrumlabs");
        }
        else
        {
            logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), // maps to ~/.local/share
                "fulcrumlabs"
            );
        }

        Directory.CreateDirectory(logDir);
        return logDir;
    }
}