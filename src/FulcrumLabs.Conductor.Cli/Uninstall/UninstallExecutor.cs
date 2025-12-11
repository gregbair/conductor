using Renci.SshNet;

using Spectre.Console;

namespace FulcrumLabs.Conductor.Cli.Uninstall;

/// <summary>
///     Executor to uninstall the agent.
/// </summary>
public class UninstallExecutor : BaseExecutor
{
    /// <summary>
    ///     Executes an uninstall on the given host
    /// </summary>
    /// <param name="host">The host to uninstall from</param>
    /// <param name="username">The username to connect to the <paramref name="host" /></param>
    /// <param name="sudoPassword">The sudo password to use</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>An exit code (for now)</returns>
    public async Task<int> ExecuteUninstall(string host, string username, string sudoPassword,
        CancellationToken cancellationToken = default)
    {
        string hostDisplay = $"[bold yellow]({host})[/]";

        AnsiConsole.MarkupLine($"Uninstalling agent in directory {AgentDir}... {hostDisplay}");

        using SshClient sshClient = CreateSshClient(host, username);
        await sshClient.ConnectAsync(cancellationToken);

        ExecuteWithSudoAsync(sshClient, $"rm -fr {AgentDir}", sudoPassword);
        AnsiConsole.MarkupLine($"Agent successfully uninstalled {hostDisplay}");
        return 0;
    }
}