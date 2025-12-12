// See https://aka.ms/new-console-template for more information

using System.Threading;

using FulcrumLabs.Conductor.Cli.Agent.Version;
using FulcrumLabs.Conductor.Core.Util;

using Spectre.Console.Cli;

CommandApp app = new();
app.Configure(config =>
{
    config.AddCommand<VersionCommand>("version");
    config.PropagateExceptions();
});

CancellationTokenSource cts = CancellationTokenSourceUtils.CreateProcessShutdownTokenSource();

return await app.RunAsync(args);