using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using FulcrumLabs.Conductor.Core.Modules;

using Spectre.Console.Cli;

namespace FulcrumLabs.Conductor.Agent.Cli.Version;

/// <summary>
///     Runs the 'version' command
/// </summary>
public class VersionCommand : AsyncCommand
{
    /// <inheritdoc />
    public override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        VersionResult result = new();
        result.ModuleSearchPaths.Add("modules");

        ModuleRegistry moduleRegistry = new();
        moduleRegistry.DiscoverModules("modules");

        ModuleExecutor executor = new(moduleRegistry);
        foreach (string name in moduleRegistry.GetModuleNames())
        {
            ModuleVersionInfo verResult = await executor.GetModuleVersionAsync(name, cancellationToken);
            result.Modules.Add(name, verResult.ModuleVersion.ToString());
        }

        string json = JsonSerializer.Serialize(result,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
        Console.WriteLine(json);

        return 0;
    }
}