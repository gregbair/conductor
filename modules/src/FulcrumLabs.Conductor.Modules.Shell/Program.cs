using System.Threading;

using FulcrumLabs.Conductor.Modules.Common;
using FulcrumLabs.Conductor.Modules.Shell;

CancellationTokenSource cts = ModuleBase.CreateShutdownTokenSource();
return await new ShellModule().RunAsync(cts.Token);