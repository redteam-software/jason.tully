namespace RedTeamGoCli.Commands.Config;

public record SshConfigParameters(

    string? path = null,
    string? env = null,
    string? logLevel = "error") : BaseCommandParameters(path, env, logLevel);

/// <summary>
/// Parses and displays all configured SSH hosts from the ~/.ssh/config file.
/// Shows host aliases and their corresponding hostnames in a formatted panel view.
/// Optionally accepts a custom path to an alternative SSH config file.
/// </summary>
[SubCommand(SubCommandConfig.SubCommandName, SubCommandConfig.SubCommandDescription)]
[SubCommandHandler(
   SubCommandConfig.SubCommandName,
   SubCommandConfig.CommandSshConfigsList.CommandName,
   SubCommandConfig.CommandSshConfigsList.CommandDescription)]
public class SshConfigCommand : ICommand<SshConfigParameters>
{
    private readonly ISshConfigParser _sshConfigParser;

    public SshConfigCommand(ISshConfigParser sshConfigParser)
    {
        _sshConfigParser = sshConfigParser;
    }

    public async Task RunAsync(SshConfigParameters args, CommandContext commandContext, CancellationToken cancellationToken = default)
    {
        SubCommandConfig.CommandSshConfigsList.Format().WriteSubTitle(false);

        var results = _sshConfigParser.Parse(args.path);

        var rows = new Rows(results.Select(x => new Markup($"{x.Host.Information()} - {x.Options["HostName"]}")).ToArray());
        var panel = new Panel(rows).Expand();
        panel.Header("Current SSH Hosts");

        AnsiConsole.Write(panel);
    }
}