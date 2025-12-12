// See https://aka.ms/new-console-template for more information

using FulcrumLabs.Conductor.Cli.Common;
using FulcrumLabs.Conductor.Cli.Executor.Install;
using FulcrumLabs.Conductor.Cli.Executor.Uninstall;
using FulcrumLabs.Conductor.Core.Util;

using Spectre.Console.Cli;

CommandApp app = new();

app.Configure(config =>
{
    config.SetInterceptor(new VerbosityInterceptor());
    config.AddCommand<InstallCommand>("install");
    config.AddCommand<UninstallCommand>("uninstall");
});

CancellationTokenSource cts = CancellationTokenSourceUtils.CreateProcessShutdownTokenSource();

return await app.RunAsync(args, cts.Token);