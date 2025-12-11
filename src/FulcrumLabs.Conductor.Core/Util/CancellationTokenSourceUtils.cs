using System.Runtime.InteropServices;

namespace FulcrumLabs.Conductor.Core.Util;

/// <summary>
///     Contains utility methods for <see cref="CancellationTokenSource" />s
/// </summary>
public static class CancellationTokenSourceUtils
{
    /// <summary>
    ///     Creates a <see cref="CancellationTokenSource" /> that listens to shutdown signals
    /// </summary>
    /// <returns>A <see cref="CancellationTokenSource" /></returns>
    public static CancellationTokenSource CreateProcessShutdownTokenSource()
    {
        CancellationTokenSource cts = new();

        // SIGTERM works on Unix-like platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            PosixSignalRegistration.Create(PosixSignal.SIGTERM, ctx =>
            {
                ctx.Cancel = true;
                cts.Cancel();
            });
        }

        // SIGINT works everywhere
        PosixSignalRegistration.Create(PosixSignal.SIGINT, ctx =>
        {
            ctx.Cancel = true;
            cts.Cancel();
        });

        // Windows-specific Ctrl+Break
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            PosixSignalRegistration.Create(PosixSignal.SIGQUIT, ctx =>
            {
                ctx.Cancel = true;
                cts.Cancel();
            });
        }

        return cts;
    }
}