using System.Collections.Concurrent;

using Serilog;

using Spectre.Console.Cli;

namespace FulcrumLabs.Conductor.Cli.Executor.Install;

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

        if (settings.User == null)
        {
            Log.Error("User not specified.");
            return -1;
        }

        InstallExecutor executor = new();

        ConcurrentBag<int> results = [];
        // foreach host, run executor
        await Parallel.ForEachAsync(settings.Hosts, new ParallelOptions { MaxDegreeOfParallelism = settings.Parallel },
            async (host, token) =>
            {
                int result = await executor.ExecuteInstallationAsync(host, settings.User ?? "",
                    settings.SudoPassword ?? "",
                    token);

                results.Add(result);
            });

        int result = results.Any(x => x != 0) ? 1 : 0;

        // output results
        return result;
    }
}