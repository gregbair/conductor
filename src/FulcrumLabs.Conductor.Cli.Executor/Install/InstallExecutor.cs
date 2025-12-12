using System.Formats.Tar;
using System.IO.Compression;
using System.Text.Json;

using FulcrumLabs.Conductor.Agent.Cli.Version;
using FulcrumLabs.Conductor.Core.Modules;

using Renci.SshNet;

using Serilog;

using Spectre.Console;

namespace FulcrumLabs.Conductor.Cli.Executor.Install;

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
        string hostColor = GetRandomColor();
        string hostDisplay = $"[bold italic underline {hostColor}]({host}):[/]";
        using TempDirectory tempDir = new("conductor-agent-");
        // Find all modules


        OutputLine($"{hostDisplay} Finding all modules");
        ModuleRegistry registry = new();
        registry.DiscoverModulesFromStandardPaths();

        OutputLine($"{hostDisplay} Modules found:");

        Table modTable = new();
        modTable.AddColumn("Name");
        modTable.AddColumn("Path");

        foreach (string name in registry.GetModuleNames())
        {
            string? path = registry.GetModulePath(name);
            modTable.AddRow($"[blue]{name}[/]", $"[green]{path}[/]");
            Log.Logger.Debug("({host}): Module {name} found at {path}", host, name, path);
        }

        OutputTableToConsole(modTable);

        // Package modules + agent
        OutputLine($"{hostDisplay} Packing agent and modules...");
        CopyModules(registry, tempDir.Path);
        string bundlePath = await CreateBundle(tempDir.Path, Path.Combine(tempDir.Path, "bundle"), cancellationToken);
        string bundleOutput = $"{hostDisplay} Bundle packed successfully - {bundlePath}";
        OutputLine(bundleOutput);

        // SSH to node, create /opt/conductor if not exists
        OutputLine($"{hostDisplay} Verifying existence of {AgentDir}");
        using SshClient sshClient = CreateSshClient(host, username);
        try
        {
            await sshClient.ConnectAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Logger.Error("Error connecting to {host} via ssh. Exception: {ex}", host, ex);
            return -1;
        }

        bool dirExists = AgentDirExists(sshClient);

        if (!dirExists)
        {
            OutputLine($"{hostDisplay} {AgentDir} doesn't exist, creating...");
            CreateAgentDir(sshClient, sudoPassword);
            dirExists = AgentDirExists(sshClient);
            if (!dirExists)
            {
                // couldn't create the dir
                Log.Logger.Error("Could not create directory {AgentDir}", AgentDir);
                return -1;
            }

            OutputLine($"{hostDisplay} {AgentDir} created");
        }
        else
        {
            OutputLine($"{hostDisplay} {AgentDir} exists");
        }


        // SCP over bundle
        using ScpClient scpClient = CreateScpClient(host, username);
        try
        {
            await scpClient.ConnectAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Logger.Error("Error connecting to {host} via ssh. Exception: {ex}", host, ex);
            return -1;
        }

        OutputLine($"{hostDisplay} Copying bundle over to managed node...");
        CopyFileOverToRemoteHost(scpClient, bundlePath);
        OutputLine($"{hostDisplay} Bundle copied over to managed node");

        // Use SSH connection to unpack bundle
        OutputLine($"{hostDisplay} Unpacking and installing bundle...");
        UnpackAndCopyBundle(sshClient, sudoPassword);
        OutputLine($"{hostDisplay} Bundle installed");

        // check version of agent
        OutputLine($"{hostDisplay} Verifying installation...");
        VersionResult? version = GetAgentVersion(sshClient);
        OutputLine($"{hostDisplay} Verified  version {version?.AgentVersion ?? "(no version found)"}");

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