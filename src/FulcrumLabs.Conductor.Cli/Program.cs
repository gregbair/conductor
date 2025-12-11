// See https://aka.ms/new-console-template for more information

using FulcrumLabs.Conductor.Cli.Install;
using FulcrumLabs.Conductor.Cli.Uninstall;
using FulcrumLabs.Conductor.Core.Util;

using Spectre.Console.Cli;

CommandApp app = new();

app.Configure(config =>
{
    config.AddCommand<InstallCommand>("install");
    config.AddCommand<UninstallCommand>("uninstall");
});

CancellationTokenSource cts = CancellationTokenSourceUtils.CreateProcessShutdownTokenSource();

return await app.RunAsync(args, cts.Token);