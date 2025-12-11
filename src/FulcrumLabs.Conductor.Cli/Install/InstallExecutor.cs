using System.Formats.Tar;
using System.IO.Compression;
using System.Text.Json;

using FulcrumLabs.Conductor.Agent.Cli.Version;
using FulcrumLabs.Conductor.Core.Modules;

using Renci.SshNet;

using Spectre.Console;

namespace FulcrumLabs.Conductor.Cli.Install;

/// <summary>
///     Executes agent installation to a host
/// </summary>
public class InstallExecutor : BaseExecutor
{
    private const string RemoteBundlePath = "bundle.tgz";

    /// <summary>
    ///     Executes the installation
    /// </summary>
    /// <param name="host">The host to install to</param>
    /// <param name="username">The username to connect to the <paramref name="host" /></param>
    /// <param name="sudoPassword">The sudo password to use</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>An exit code (for now)</returns>
    public async Task<int> ExecuteInstallationAsync(string host, string username, string sudoPassword,
        CancellationToken cancellationToken)
    {
        string hostDisplay = $"[bold yellow]({host})[/]";
        using TempDirectory tempDir = new("conductor-agent-");
        // Find all modules
        AnsiConsole.MarkupLine($"Finding all modules {hostDisplay}");
        ModuleRegistry registry = new();
        registry.DiscoverModulesFromStandardPaths();

        Table modTable = new();
        modTable.AddColumn("Name");
        modTable.AddColumn("Path");

        foreach (string name in registry.GetModuleNames())
        {
            modTable.AddRow($"[blue]{name}[/]", $"[green]{registry.GetModulePath(name)}[/]");
        }

        AnsiConsole.MarkupLine($"Modules found {hostDisplay}:");
        AnsiConsole.Write(modTable);

        // Package modules + agent
        AnsiConsole.MarkupLine($"Packing agent and modules... {hostDisplay}");
        CopyModules(registry, tempDir.Path);
        string bundlePath = await CreateBundle(tempDir.Path, Path.Combine(tempDir.Path, "bundle"), cancellationToken);
        AnsiConsole.Markup($"Bundle packed successfully {hostDisplay} ");
        AnsiConsole.Write(new TextPath(bundlePath));
        AnsiConsole.WriteLine();

        // SSH to node, create /opt/conductor if not exists
        AnsiConsole.MarkupLine($"Verifying existence of {AgentDir} {hostDisplay}");
        SshClient sshClient = CreateSshClient(host, username);
        await sshClient.ConnectAsync(cancellationToken);

        bool dirExists = AgentDirExists(sshClient);

        if (!dirExists)
        {
            AnsiConsole.MarkupLine($"{AgentDir} doesn't exist, creating... {hostDisplay}");
            CreateAgentDir(sshClient, sudoPassword);
            dirExists = AgentDirExists(sshClient);
            if (!dirExists)
            {
                // couldn't create the dir
                return -1;
            }

            AnsiConsole.MarkupLine($"{AgentDir} created {hostDisplay}");
        }
        else
        {
            AnsiConsole.MarkupLine($"{AgentDir} exists {hostDisplay}");
        }


        // SCP over bundle
        using ScpClient scpClient = CreateScpClient(host, username);
        await scpClient.ConnectAsync(cancellationToken);

        AnsiConsole.MarkupLine($"Copying bundle over to managed node... {hostDisplay}");
        CopyFileOverToRemoteHost(scpClient, bundlePath);
        AnsiConsole.MarkupLine($"Bundle copied over to managed node {hostDisplay}");

        // Use SSH connection to unpack bundle
        AnsiConsole.MarkupLine($"Unpacking and installing bundle... {hostDisplay}");
        UnpackAndCopyBundle(sshClient, sudoPassword);
        AnsiConsole.MarkupLine($"Bundle installed {hostDisplay}");

        // check version of agent
        AnsiConsole.MarkupLine($"Verifying installation... {hostDisplay}");
        VersionResult? version = GetAgentVersion(sshClient);
        AnsiConsole.MarkupLine($"Verified  version {version?.AgentVersion ?? "(no version found)"} {hostDisplay}");

        return version == null ? -1 : 0;
    }

    private static void CreateAgentDir(SshClient sshClient, string sudoPassword)
    {
        ExecuteWithSudoAsync(sshClient, $"mkdir -p {AgentDir}", sudoPassword);
    }

    private static bool AgentDirExists(SshClient sshClient)
    {
        SshCommand cmd = sshClient.RunCommand("ls /opt");

        string[] dirs = cmd.Result.Split("\n").Select(item => Path.Combine("/opt", item)).ToArray();

        return dirs.Any(x => x == AgentDir);
    }

    private static VersionResult? GetAgentVersion(SshClient sshClient)
    {
        SshCommand cmd = sshClient.RunCommand(Path.Combine(AgentDir, "conductor-agent") + " version");

        try
        {
            return JsonSerializer.Deserialize<VersionResult>(cmd.Result, JsonOpts);
        }
        catch
        {
            return null;
        }
    }

    private static void CopyFileOverToRemoteHost(ScpClient client, string bundlePath)
    {
        client.Upload(new FileInfo(bundlePath), RemoteBundlePath);
    }

    private static void UnpackAndCopyBundle(SshClient client, string sudoPassword)
    {
        ExecuteWithSudoAsync(client, $"tar -xzf {RemoteBundlePath} -C {AgentDir}", sudoPassword);
        client.RunCommand($"rm {RemoteBundlePath}");
    }

    private static void CopyModules(ModuleRegistry moduleRegistry, string tempDir)
    {
        Directory.CreateDirectory(Path.Combine(tempDir, "bundle", "modules"));
        foreach (string name in moduleRegistry.GetModuleNames())
        {
            string? path = moduleRegistry.GetModulePath(name);
            if (path is null)
            {
                continue;
            }

            File.Copy(path, Path.Combine(tempDir, "bundle", "modules", $"conductor-module-{name}"));
        }

        string agentPath = Path.Combine(AppContext.BaseDirectory, "conductor-agent");

        if (File.Exists(agentPath))
        {
            File.Copy(agentPath, Path.Combine(tempDir, "bundle", "conductor-agent"));
        }
    }

    private static async Task<string> CreateBundle(string tempPath, string bundlePath,
        CancellationToken cancellationToken)
    {
        const string fileName = "modules.tar.gz";

        await using FileStream fileStream = File.Create(Path.Combine(tempPath, fileName));
        await using GZipStream gzipStream = new(fileStream, CompressionLevel.Optimal);
        await using TarWriter archive = new(gzipStream);

        foreach (string file in Directory.GetFiles(bundlePath, "*", SearchOption.AllDirectories))
        {
            await archive.WriteEntryAsync(file, Path.GetRelativePath(bundlePath, file), cancellationToken);
        }

        return Path.Combine(tempPath, fileName);
    }
}