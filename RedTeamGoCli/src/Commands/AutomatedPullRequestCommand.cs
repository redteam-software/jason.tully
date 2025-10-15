using RedTeam.Extensions.Console.ExtensionMethods;
using System.Diagnostics;

namespace RedTeamGoCli.Commands;

public record AutomatedPullRequestParameters(
   [Option("target", Description = "The target branch to create the pull request against. If not specified, the default branch will be used.")] string? targetBranch = "uatcode_v3",
  string? logLevel = "error") : CommandParameters(logLevel);

[CommandHandler("auto-pr", "Using the current project directory,  creates a pull request, auto merges it and displays the progress of the associated git hub acction.")]
public class AutomatedPullRequestCommand : ICommand<AutomatedPullRequestParameters>
{
    private readonly IGitClient _gitClient;
    private readonly IGitHubCli _gitHubCli;

    public AutomatedPullRequestCommand(IGitClient gitClient, IGitHubCli gitHubCli)
    {
        _gitClient = gitClient;
        _gitHubCli = gitHubCli;
    }
    public async Task RunAsync(AutomatedPullRequestParameters args, CommandContext commandContext, CancellationToken cancellationToken = default)
    {
        "auto-pr - Automates creating, merging and monitoring Go pull requests".WriteSubTitle(false);
        if (Debugger.IsAttached)
        {
            "Debugger is attached, setting current directory to 'D:\\Development\\RedTeam\\Go\\go-production-v4".Warning().WriteLine();
            Environment.CurrentDirectory = @"D:\Development\RedTeam\Go\go-production-v4";
        }


        var currentBranch = _gitClient.GetCurrentBranchName()!;

        var targetBranch = args.targetBranch;

        if (string.IsNullOrWhiteSpace(targetBranch))
        {
            targetBranch = "uatcode_v3";
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
                     prTask.PostTaskMessage(new RedTeam.Extensions.Console.Models.ProgressTaskMessage("PR Created", 200, ""));
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



