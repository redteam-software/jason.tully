namespace RedTeamGoCli.Commands;

public record AutomatedPullRequestParameters(
   [Option("target", Description = "The target branch to create the pull request against. If not specified, the default branch will be used.")] string? targetBranch,
     [Option(Common.Path, Description = Common.PathDescription)] string? path = null,
   string? logLevel = "error") : CommandParameters(logLevel);


[SubCommand(SubCommandGit.SubCommandName, SubCommandGit.SubCommandDescription)]
[SubCommandHandler(
   SubCommandGit.SubCommandName,
   SubCommandGit.CommandAutomatedPullRequest.CommandName,
   SubCommandGit.CommandAutomatedPullRequest.CommandDescription)]
public class AutomatedPullRequestCommand : ICommand<AutomatedPullRequestParameters>
{
    private readonly IGitService _gitService;
    private readonly IGoProjectFactory _goProjectFactory;
    private readonly IGitHubCli _gitHubCli;

    public AutomatedPullRequestCommand(IGitService gitService,
         IGoProjectFactory goProjectFactory,
        IGitHubCli gitHubCli)
    {
        _gitService = gitService;
        _goProjectFactory = goProjectFactory;
        _gitHubCli = gitHubCli;
    }
    public async Task RunAsync(AutomatedPullRequestParameters args, CommandContext commandContext, CancellationToken cancellationToken = default)
    {
        SubCommandGit.CommandAutomatedPullRequest.Format().WriteSubTitle(false);



        if (!string.IsNullOrEmpty(args.path) && Directory.Exists(args.path))
        {
            Environment.CurrentDirectory = args.path;
        }

        var project = _goProjectFactory.GetProjectFromDirectory<IGoAutomaticPullRequestProject>(Environment.CurrentDirectory);
        if (project == null)
        {
            $"{Environment.CurrentDirectory.TextValue(true).Error()} is not a valid Go Laravel Project".WriteLine();
            return;
        }

        var currentBranch = _gitService.GetCurrentBranchName()!;

        var targetBranch = args.targetBranch;

        if (string.IsNullOrWhiteSpace(targetBranch))
        {

            targetBranch = project.TargetBranch;
        }



        var taskDescriptionColumn = new TaskDescriptionColumn();
        taskDescriptionColumn.Alignment = Justify.Left;
        await AnsiConsole.Progress().AutoRefresh(true)
             .Columns(new ProgressColumn[]
             {
                 taskDescriptionColumn,
                 new SpectreConsoleProgressTaskMessageColumn(),
                 new SpinnerColumn()
             }).StartAsync(async ctx =>
             {
                 var prTask = ctx.AddTask($"Create PR For {currentBranch}");
                 var mergeTask = ctx.AddTask("Auto-Merging PR");
                 var runActionTask = ctx.AddTask("Waiting For GitHub Action To Complete.");

                 prTask.IsIndeterminate = true;
                 mergeTask.IsIndeterminate = true;
                 runActionTask.IsIndeterminate = true;

                 ctx.Refresh();

                 var prResult = await _gitHubCli.CreatePullRequest(targetBranch, currentBranch, false);
                 if (prResult.Status)
                 {
                     prTask.PostTaskMessage(new RedTeam.Extensions.Console.Models.ProgressTaskMessage($"PR Created {prResult.Message!.TextValue(true)}", 200, ""));
                     prTask.StopTask();

                     var mergeResult = await _gitHubCli.MergePullRequest();
                     mergeTask.StopTask();
                     if (!mergeResult.Status)
                     {
                         mergeTask.PostTaskMessage(new RedTeam.Extensions.Console.Models.ProgressTaskMessage(mergeResult.Message ?? "Error", 500, ""));

                         runActionTask.StopTask();
                     }
                     else
                     {
                         mergeTask.PostTaskMessage(new RedTeam.Extensions.Console.Models.ProgressTaskMessage("PR Merged", 200, ""));
                         var actionResult = await _gitHubCli.MonitorDeploymentAsync(runActionTask);
                         runActionTask.StopTask();
                     }
                 }
                 else
                 {
                     prTask.PostTaskMessage(new RedTeam.Extensions.Console.Models.ProgressTaskMessage(prResult.Message ?? "Error", 500, ""));
                     prTask.StopTask();
                     mergeTask.StopTask();
                     runActionTask.StopTask();
                 }





             });

    }

}



