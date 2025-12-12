using Renci.SshNet;

using Serilog.Core;

namespace FulcrumLabs.Conductor.Cli.Executor.Uninstall;

/// <summary>
///     Executor to uninstall the agent.
/// </summary>
public class UninstallExecutor : BaseExecutor
{
    /// <summary>
    ///     Initializes a new <see cref="UninstallExecutor" />
    /// </summary>
    /// <param name="consoleLogger">The console logger to use</param>
    public UninstallExecutor(Logger consoleLogger) : base(consoleLogger)
    {
    }

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
        string hostColor = GetRandomColor();
        string hostDisplay = $"[bold italic underline {hostColor}]({host}):[/]";

        OutputLineToConsoleAndFile($"{hostDisplay} Uninstalling agent in directory {AgentDir}...");

        using SshClient sshClient = CreateSshClient(host, username);
        await sshClient.ConnectAsync(cancellationToken);

        ExecuteWithSudoAsync(sshClient, $"rm -fr {AgentDir}", sudoPassword);
        OutputLineToConsoleAndFile($"{hostDisplay} Agent successfully uninstalled");
        return 0;
    }
}