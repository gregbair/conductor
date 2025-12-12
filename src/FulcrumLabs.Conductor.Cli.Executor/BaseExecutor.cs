using System.Text.Json;
using System.Text.RegularExpressions;

using Renci.SshNet;

using Serilog;

using Spectre.Console;

namespace FulcrumLabs.Conductor.Cli.Executor;

/// <summary>
///     Base for all executor classes
/// </summary>
public abstract class BaseExecutor
{
    /// <summary>
    ///     Options to use when serializing JSON
    /// </summary>
    protected static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    /// <summary>
    ///     The directory in which the agent is.
    /// </summary>
    protected static readonly string AgentDir = Path.Combine("/opt", "conductor");

    /// <summary>
    ///     Outputs a line using <see cref="Rule" />
    /// </summary>
    /// <param name="text">The text to use as the title of the rule</param>
    protected static void OutputLine(string text)
    {
        AnsiConsole.MarkupLine(text);
        LogPlainText(text);
    }

    /// <summary>
    ///     Converts the marked-up <paramref name="text" /> to plaintext and logs it
    /// </summary>
    /// <param name="text">The marked-up text</param>
    protected static void LogPlainText(string text)
    {
        using StringWriter writer = new();
        IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.No, ColorSystem = ColorSystemSupport.NoColors, Out = new AnsiConsoleOutput(writer)
        });

        console.Markup(text);
        Log.Logger.Information(writer.ToString());
    }

    /// <summary>
    ///     Outputs a <see cref="Table" />
    /// </summary>
    /// <param name="table">The <see cref="Table" /> to render</param>
    protected static void OutputTableToConsole(Table table)
    {
        AnsiConsole.Write(table);
    }

    /// <summary>
    ///     Executes a command with sudo
    /// </summary>
    /// <param name="client">The <see cref="SshClient" /> to use</param>
    /// <param name="command">The command to run</param>
    /// <param name="sudoPassword">The password to use for authenticating</param>
    /// <returns>The output of the command</returns>
    /// <exception cref="InvalidOperationException">Thrown if an error occurs while authenticating</exception>
    protected static string ExecuteWithSudoAsync(SshClient client, string command, string sudoPassword)
    {
        // Use ShellStream to allocate a PTY for sudo password authentication
        using ShellStream shell = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024);

        // Wait for initial prompt
        shell.Expect(new Regex(@"[$>]\s*$"), TimeSpan.FromSeconds(5));

        // Send sudo command
        shell.WriteLine($"sudo -S {command}");

        // Wait for password prompt
        shell.Expect(new Regex(@"password.*:"), TimeSpan.FromSeconds(5));

        // Send password
        shell.WriteLine(sudoPassword);

        // Wait for command to complete
        Thread.Sleep(2000);

        // Read output
        string output = shell.Read();

        // Check for authentication errors
        if (output.Contains("Sorry, try again") || output.Contains("incorrect password"))
        {
            throw new InvalidOperationException("Sudo authentication failed");
        }

        return output;
    }

    /// <summary>
    ///     Creates a new <see cref="SshClient" /> using certificate auth
    /// </summary>
    /// <param name="host">The host to connect to</param>
    /// <param name="username">The username to connect as</param>
    /// <returns>A new <see cref="SshClient" /> that has NOT been connected</returns>
    protected static SshClient CreateSshClient(string host, string username)
    {
        PrivateKeyFile keyFile = new(GetKeyFilePath());
        PrivateKeyAuthenticationMethod authMethod = new(username, keyFile);
        return new SshClient(new ConnectionInfo(host, username, authMethod));
    }

    /// <summary>
    ///     Creates a new <see cref="ScpClient" /> using certificate auth
    /// </summary>
    /// <param name="host">The host to connect to</param>
    /// <param name="username">The username to connect as</param>
    /// <returns>A new <see cref="SshClient" /> that has NOT been connected</returns>
    protected static ScpClient CreateScpClient(string host, string username)
    {
        PrivateKeyFile keyFile = new(GetKeyFilePath());
        PrivateKeyAuthenticationMethod authMethod = new(username, keyFile);
        return new ScpClient(new ConnectionInfo(host, username, authMethod));
    }

    /// <summary>
    ///     Gets a random color
    /// </summary>
    /// <returns>A random color</returns>
    protected string GetRandomColor()
    {
        string[] colors =
        [
            "maroon", "green", "olive", "navy", "purple", "teal", "silver", "grey", "red", "lime", "yellow", "blue",
            "fuchsia", "aqua", "white"
        ];

        return colors[Random.Shared.Next(colors.Length)];
    }

    private static string GetKeyFilePath()
    {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string[] defaultKeys = [Path.Combine(home, ".ssh", "id_rsa"), Path.Combine(home, ".ssh", "id_ed25519")];

        string keyPath = "";
        foreach (string k in defaultKeys)
        {
            if (!File.Exists(k))
            {
                continue;
            }

            keyPath = k;
            break;
        }

        return string.IsNullOrEmpty(keyPath)
            ? throw new InvalidOperationException("Could not find private SSH key")
            : keyPath;
    }
}