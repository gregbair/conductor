using System.Threading;

using FulcrumLabs.Conductor.Modules.Common;
using FulcrumLabs.Conductor.Modules.Debug;

CancellationTokenSource cts = ModuleBase.CreateShutdownTokenSource();
return await new DebugModule().RunAsync(cts.Token);