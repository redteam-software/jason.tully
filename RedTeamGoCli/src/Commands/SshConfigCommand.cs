namespace RedTeamGoCli.Commands;

public record SshConfigParameters(

      [Option(Common.Path, Description = Common.PathDescription)] string? path = null,
      string? logLevel = "none") : CommandParameters(logLevel);


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
