using Spectre.Console.Cli;

namespace FulcrumLabs.Conductor.Cli.Install;

/// <summary>
///     Command to install an agent
/// </summary>
public class InstallCommand : AsyncCommand<InstallCommandSettings>
{
    /// <inheritdoc />
    public override async Task<int> ExecuteAsync(
        CommandContext context,
        InstallCommandSettings settings,
        CancellationToken cancellationToken)
    {
        // TODO: load and parse cfg file (YAML for now)

        InstallExecutor executor = new();

        // foreach host, run executor
        foreach (string host in settings.Hosts)
        {
            await executor.ExecuteInstallationAsync(host, settings.User ?? "", settings.SudoPassword ?? "",
                cancellationToken);
        }

        int result = 0;

        // output results
        return result;
    }
}