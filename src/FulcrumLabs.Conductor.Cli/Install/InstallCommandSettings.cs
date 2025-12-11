using System.ComponentModel;

using Spectre.Console.Cli;

namespace FulcrumLabs.Conductor.Cli.Install;

/// <summary>
///     Settings for the install command
/// </summary>
public class InstallCommandSettings : CommandSettings
{
    /// <summary>
    ///     Gets or sets the host to run the install on
    /// </summary>
    [CommandArgument(0, "[host]")]
    [Description("Specifies hostnames to install the agent to")]
    public string[] Hosts { get; set; } = [];

    /// <summary>
    ///     Gets or sets the degree of parallelism
    /// </summary>
    [CommandOption("-p|--parallel <DEGREE>")]
    [Description("Specifies the degree of parallelism")]
    public int Parallel { get; set; } = 10;

    /// <summary>
    ///     Gets or sets the sudo password
    /// </summary>
    [CommandOption("-s|--sudoPassword <PASSWORD>")]
    [Description("Specifies the password to use for elevated operations")]
    public string? SudoPassword { get; set; }

    /// <summary>
    ///     Gets or sets the user to use for SSH connections
    /// </summary>
    [CommandOption("-u|--username <USERNAME>")]
    [Description("Specifies the username to use for SSH connections")]
    public string? User { get; set; }

    /// <summary>
    ///     Gets the user to use for SSH connections
    /// </summary>
    /// <returns>Either the <see cref="User" /> or the current logged-in user</returns>
    public string GetUser()
    {
        return User ?? Environment.UserName;
    }
}