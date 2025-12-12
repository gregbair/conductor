using System.Threading;

using FulcrumLabs.Conductor.Core.Util;
using FulcrumLabs.Conductor.Modules.Debug;

CancellationTokenSource cts = CancellationTokenSourceUtils.CreateProcessShutdownTokenSource();
return await new DebugModule().RunAsync(cts.Token);