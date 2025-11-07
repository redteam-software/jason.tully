using RedTeamGoCli.Utilities;

namespace RedTeamGoCli.Commands.Logs;

public record ListRemoteLogCommandParameters(

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
   SubCommandLogs.CommanListRemoteLogs.CommandName,
   SubCommandLogs.CommanListRemoteLogs.CommandDescription)]
public class ListRemoteLogCommandCommand : ICommand<ListRemoteLogCommandParameters>
{
    private readonly ISshService _sSHLogService;
    private readonly IGoProjectFactory _goProjectFactory;

    public ListRemoteLogCommandCommand(ISshService sSHLogService,

        IGoProjectFactory goProjectFactory)
    {
        _sSHLogService = sSHLogService;
        _goProjectFactory = goProjectFactory;
    }

    public Task RunAsync(ListRemoteLogCommandParameters args, CommandContext commandContext, CancellationToken cancellationToken = default)
    {
        var result = Task.CompletedTask;
        SubCommandLogs.CommanListRemoteLogs.Format().WriteSubTitle(false);
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

        var remoteServiceProject = _goProjectFactory.GetProjectFromDirectory<IGoRemoteServiceProject>(args.env, Environment.CurrentDirectory);
        if (remoteServiceProject == null)
        {
            $"{Environment.CurrentDirectory.TextValue(true).Error()} is not a valid Go Remote Service Project".WriteLine();
            return result;
        }

        var response = _sSHLogService.ListRemoteLogCommand(project);

        var choices = response ?? new List<string>();

        var choice = RedTeam.Extensions.Console.UserInput.ChoicePrompt.Prompt(
             "Remote Log Files:",
             choices.Distinct().OrderByDescending(x => x),
             30);


        var remoteService = _sSHLogService as IRemoteService;

        if (remoteService == null)
        {
            $"This project does not support remove file downloads.".WriteLine();
            return result;
        }

        var downloadResult = remoteService.DownloadRemoteFile(project.RemoteLogPath + "/" + choice, choice, remoteServiceProject, DefaultNotificationService.Instance, true);

        if (!string.IsNullOrWhiteSpace(downloadResult.Result.LocalPath))
        {
            $"Downloaded: {downloadResult.Result.LocalPath.TextValue(true)}".WriteLine();
            VsCodeUtility.OpenFile(downloadResult.Result.LocalPath);
        }
        return result;
    }
}