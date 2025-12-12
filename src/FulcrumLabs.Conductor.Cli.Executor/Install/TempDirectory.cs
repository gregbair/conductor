namespace FulcrumLabs.Conductor.Cli.Executor.Install;

/// <summary>
///     Creates and removes a temp directory (OS-agnostic)
/// </summary>
public class TempDirectory : IDisposable
{
    /// <summary>
    ///     Initializes a new instance of <see cref="TempDirectory" />
    /// </summary>
    /// <param name="prefix"></param>
    public TempDirectory(string? prefix = null)
    {
        Path = Directory.CreateTempSubdirectory(prefix ?? "temp-").FullName;
    }

    /// <summary>
    ///     The path of the temp directory
    /// </summary>
    public string Path { get; }

    /// <summary>
    ///     Deletes the temp directory if it exists
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(Path))
        {
            Directory.Delete(Path, true);
        }

        GC.SuppressFinalize(this);
    }
}