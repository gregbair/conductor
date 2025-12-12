using System.ComponentModel;

using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

using Spectre.Console.Cli;

namespace FulcrumLabs.Conductor.Cli.Common;

/// <summary>
///     Base settings for all commands
/// </summary>
public abstract class BaseCommandSettings : CommandSettings
{
    /// <summary>
    ///     Gets or sets log verbosity
    /// </summary>
    [CommandOption("-v|--verbose")]
    [Description("Increase verbosity (-v, -vv, -vvv, -vvvv)")]
    public int Verbosity { get; set; }

    /// <summary>
    ///     Converts the <see cref="Verbosity" /> into a <see cref="LogEventLevel" />
    /// </summary>
    /// <returns>A <see cref="LogEventLevel" /> corresponding to the verbosity level</returns>
    public LogEventLevel GetLogLevel()
    {
        return Verbosity switch
        {
            0 => LogEventLevel.Information, // Default
            1 => LogEventLevel.Debug, // -v: Task results
            _ => LogEventLevel.Verbose // -vv: Config info
        };
    }

    /// <summary>
    ///     Creates a console logger to use for commands
    /// </summary>
    /// <returns>The console logger</returns>
    public Logger CreateConsoleLogger()
    {
        LogEventLevel level = GetLogLevel();

        return new LoggerConfiguration()
            .MinimumLevel.Is(level)
            .WriteTo.Console(
                outputTemplate: "{Message:lj}{NewLine}{Exception}",
                theme: AnsiConsoleTheme.Code)
            .CreateLogger();
    }
}