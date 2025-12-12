using System.Threading;

using FulcrumLabs.Conductor.Core.Util;
using FulcrumLabs.Conductor.Modules.Shell;

CancellationTokenSource cts = CancellationTokenSourceUtils.CreateProcessShutdownTokenSource();
return await new ShellModule().RunAsync(cts.Token);