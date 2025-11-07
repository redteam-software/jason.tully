using RedTeamGoCli.Utilities;

namespace RedTeamGoCli.Commands.Logs;

public record GetRemoteLogParameters(

        string? path = null,
        string? env = null,
        string? logLevel = "error") : BaseCommandParameters(path, env, logLevel);

/// <summary>
/// Retrieves and downloads remote log files from Laravel Go applications via SSH.
/// Requires a valid Go Laravel Project with remote log configuration and an SSH host
/// configured in ~/.ssh/config file.
/// </summary>
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

        var project = _goProjectFactory.GetProjectFromDirectory<IGoRemoteLogProject>(args.env, Environment.CurrentDirectory);
        if (project == null)
        {
            $"{Environment.CurrentDirectory.TextValue(true).Error()} is not a valid Go Laravel Project".WriteLine();
            return result;
        }

        var response = _sSHLogService.GetRemoteLog(project);
        if (File.Exists(response))
        {

            $"Downloaded Log {response.TextValue(true)}".WriteLine();
            VsCodeUtility.OpenFile(response);
        }
        else
        {
            $"Failed to download log file.".WriteLine();
        }

        return result;
    }
}