using Spectre.Console.Cli;

namespace FulcrumLabs.Conductor.Cli.Uninstall;

/// <summary>
///     The command for uninstalling the agent
/// </summary>
public class UninstallCommand : AsyncCommand<UninstallCommandSettings>
{
    /// <inheritdoc />
    public override async Task<int> ExecuteAsync(
        CommandContext context,
        UninstallCommandSettings settings,
        CancellationToken cancellationToken)
    {
        UninstallExecutor executor = new();

        foreach (string host in settings.Hosts)
        {
            await executor.ExecuteUninstall(host, settings.User!, settings.SudoPassword!, cancellationToken);
        }

        return 0;
    }
}