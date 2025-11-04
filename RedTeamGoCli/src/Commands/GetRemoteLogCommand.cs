namespace RedTeamGoCli.Commands;

public record GetRemoteLogParameters(

      [Option(Common.Path, Description = Common.PathDescription)] string? path = null,
      string? logLevel = "none") : CommandParameters(logLevel);


[SubCommand(SubCommandLogs.SubCommandName, SubCommandLogs.SubCommandDescription)]
[SubCommandHandler(
   SubCommandLogs.SubCommandName,
   SubCommandLogs.CommandGetRemoteLogs.CommandName,
   SubCommandLogs.CommandGetRemoteLogs.CommandDescription)]
public class GetRemoteLogCommand : ICommand<GetRemoteLogParameters>
{
    private readonly ISshService _sSHLogService;
    private readonly IGoProjectFactory _goProjectFactory;

    public GetRemoteLogCommand(ISshService sSHLogService, IGoProjectFactory goProjectFactory)
    {
        _sSHLogService = sSHLogService;
        _goProjectFactory = goProjectFactory;
    }
    public Task RunAsync(GetRemoteLogParameters args, CommandContext commandContext, CancellationToken cancellationToken = default)
    {
        var result = Task.CompletedTask;
        SubCommandLogs.CommandGetRemoteLogs.Format().WriteSubTitle(false);
        if (!string.IsNullOrEmpty(args.path) && Directory.Exists(args.path))
        {
            Environment.CurrentDirectory = args.path;
        }

        var project = _goProjectFactory.GetProjectFromDirectory<IGoRemoteLogProject>(Environment.CurrentDirectory);
        if (project == null)
        {
            $"{Environment.CurrentDirectory.TextValue(true).Error()} is not a valid Go Laravel Project".WriteLine();
            return result;
        }

        var response = _sSHLogService.GetRemoteLog(project);

        $"Downloaded Log {response.TextValue(true)}".WriteLine();

        return result;

    }
}
