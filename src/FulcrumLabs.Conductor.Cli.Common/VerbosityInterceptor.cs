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

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(level)
            .WriteTo.Console(
                outputTemplate: "{Message:lj}{NewLine}{Exception}",
                theme: AnsiConsoleTheme.Code)
            .CreateLogger();
    }
}