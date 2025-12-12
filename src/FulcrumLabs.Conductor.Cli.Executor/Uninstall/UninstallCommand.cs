using System.Collections.Concurrent;

using Spectre.Console.Cli;

namespace FulcrumLabs.Conductor.Cli.Executor.Uninstall;

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
        UninstallExecutor executor = new(settings.CreateConsoleLogger());

        ConcurrentBag<int> results = [];
        await Parallel.ForEachAsync(settings.Hosts, cancellationToken, async (host, token) =>
        {
            int result = await executor.ExecuteUninstall(host, settings.User!, settings.SudoPassword!, token);

            results.Add(result);
        });

        return results.Any(x => x != 0) ? 1 : 0;
    }
}