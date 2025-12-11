using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace FulcrumLabs.Conductor.Agent.Cli.Version;

/// <summary>
///     The Version object returned by the agent
/// </summary>
public class VersionResult
{
    /// <summary>
    ///     Gets the agent's version
    /// </summary>
    public string AgentVersion { get; } = Assembly.GetEntryAssembly()!
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "0.0.0";

    /// <summary>
    ///     Gets the agent protocol version. Hardcoded at 1.0 for now
    /// </summary>
    public string ProtocolVersion { get; } = "1.0.0";

    /// <summary>
    ///     Gets the platform the agent is running on (e.g. linux-x64)
    /// </summary>
    public string Platform { get; } = GetPlatform();

    /// <summary>
    ///     Gets the runtime version of .NET
    /// </summary>
    public string DotnetRuntime { get; } = Environment.Version.ToString();

    /// <summary>
    ///     Gets a key-value pair of modules and their versions
    /// </summary>
    public Dictionary<string, string> Modules { get; } = [];

    /// <summary>
    ///     Gets the module search paths
    /// </summary>
    public List<string> ModuleSearchPaths { get; } = [];

    /// <summary>
    ///     Gets the datetime at which the agent was last installed
    /// </summary>
    public string InstalledAt { get; } = File
        .GetLastWriteTimeUtc(Path.Combine(AppContext.BaseDirectory, "conductor-agent"))
        .ToString("yyyy-MM-ddTHH:mm:ss.fffZ");


    private static string GetPlatform()
    {
        string os = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux" :
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" :
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows" :
            RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD) ? "freeBSD" :
            "unknown";

        string arch = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm => "arm",
            Architecture.Arm64 => "arm64",
            _ => "unknown"
        };

        return $"{os}-{arch}";
    }
}