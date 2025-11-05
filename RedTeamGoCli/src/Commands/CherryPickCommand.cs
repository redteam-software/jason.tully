using RedTeam.Extensions.Console.ShellInterop;

namespace RedTeamGit.Commands;

public record CherryPickParameters(

      [Option(Common.Author, Description = Common.AuthorDescription)] string author,
      [Option(Common.HashStart, Description = Common.HashStartDescription)] string? commitStart = null,
      [Option(Common.HashEnd, Description = Common.HashEndDescription)] string? commitEnd = null,
      [Option(Common.Since, Description = Common.SinceDescription)] string? since = null,
      [Option(Common.Until, Description = Common.UntilDescription)] string? until = null,
      [Option(Common.Path, Description = Common.PathDescription)] string? path = null,
      [Option(Common.AllBranches, Description = Common.AllBranchesDescription)] bool? all = null,
      [Option(Common.Interactive, Description = Common.InteractiveDescription)] bool? interactive = null,
      string? logLevel = "none") : CommandParameters(logLevel);

/// <summary>
/// Cherry-picks commits from a specific author within a given commit or date range, applying each commit individually
/// while excluding merge commits. Supports both automated mode (applies all commits) and interactive mode
/// (allows manual selection via UI). Displays progress table with commit details and application status.
/// </summary>
[SubCommand(SubCommandGit.SubCommandName, SubCommandGit.SubCommandDescription)]
[SubCommandHandler(
   SubCommandGit.SubCommandName,
   SubCommandGit.CommandCherryPick.CommandName,
   SubCommandGit.CommandCherryPick.CommandDescription)]
public class CherryPickCommand : ICommand<CherryPickParameters>
{
    private readonly IGitService _gitService;

    public CherryPickCommand(IShell shell, IGitService gitService)
    {
        _gitService = gitService;
    }

    public async Task RunAsync(CherryPickParameters args, CommandContext commandContext, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(args.path) && Directory.Exists(args.path))
        {
            Environment.CurrentDirectory = args.path;
        }

        if (args.interactive == true)
        {
            await _RunInteractiveAsync(args, commandContext, cancellationToken);
        }
        else
        {
            await _RunAsync(args, commandContext, cancellationToken);
        }
    }

    private async Task _RunAsync(CherryPickParameters args, CommandContext commandContext, CancellationToken cancellationToken = default)
    {
        var dateRange = DateRange.GetDateRange(args.since, args.until);
        CommitRange? commitRange = CommitRange.GetCommitRange(args.commitStart, args.commitEnd);
        var message = $"""
        Cherry Picking Commits
        {commitRange?.ToString() ?? ""}
        Author: {args.author.Information()}
        Project: {Environment.CurrentDirectory.TextValue(true)}
        """;

        message.WriteLine();

        var commitsByAuthor = _gitService.SearchCommitsByAuthor(
            args.author,
            commitRange,
            dateRange,
            GitLogFormatString.CommitInfo,
              new GitLogSearchFlags(false, true, true, args.all ?? false));

        var min = commitsByAuthor.Select(x => x.Date).Min();
        var max = commitsByAuthor.Select(x => x.Date).Max();

        $"Found {commitsByAuthor.Count.NumericValue()} commits between {min.DateTimeValue()} and {max.DateTimeValue()}".WriteLine();
        $"Applying each commit...".WriteLine();

        var table = new Table().Expand();
        table.AddColumns("Commit", "Date", "Message", "Status", "Author");
        table.Columns[4].Alignment(Justify.Right);
        table.Columns[2].Alignment(Justify.Left);
        table.Columns[1].Alignment(Justify.Center);
        table.Columns[0].Alignment(Justify.Left);
        table.AddEmptyRow();

        int counter = 0;
        AnsiConsole.Live(table).Start(ctx =>
        {
            ctx.Refresh();
            foreach (var item in commitsByAuthor)
            {
                var row = table.AddRow(item.Hash!.Substring(0, 7).Success(),
                    item.Date.DateTimeValue() ?? "-",
                    item.Message, $"Applying {item.ChangedFiles?.Count.NumericValue()} changes.", item.Author!);
                ctx.Refresh();

                var result = _gitService.CherryPickCommit(item.Hash);
                table.UpdateCell(table.Rows.Count - 1, 3, result);
                ctx.Refresh();
                counter++;
            }
        });

        $"Applied {counter.NumericValue()} commits.".Success().WriteLine();

        await Task.Delay(100);
    }

    private async Task _RunInteractiveAsync(CherryPickParameters args, CommandContext commandContext, CancellationToken cancellationToken = default)
    {
        var dateRange = DateRange.GetDateRange(args.since, args.until);
        CommitRange? commitRange = CommitRange.GetCommitRange(args.commitStart, args.commitEnd);
        var message = $"""
        Cherry Picking Commits
        {commitRange?.ToString() ?? ""}
        Author: {args.author.Information()}
        Project: {Environment.CurrentDirectory.TextValue(true)}
        """;

        message.WriteLine();

        var commitsByAuthor = _gitService.SearchCommitsByAuthor(
            args.author,
            commitRange,
            dateRange,
            GitLogFormatString.CommitInfo,
              new GitLogSearchFlags(false, true, true, args.all ?? false));

        var commits = AnsiConsole
            .Prompt(new MultiSelectionPrompt<string>()
            .Title("Select Commits To Cherry Pick")
            .Required()
            .PageSize(25)
            .AddChoices(commitsByAuthor.Select(c => $"{c.Hash} | {c.ToLocalTime()} | {c.Message}").ToArray())
         );

        foreach (var commit in commits)
        {
            var hash = commit.Split('|')[0].Trim();
            var result = _gitService.CherryPickCommit(hash);
            if (result.Contains("error", StringComparison.OrdinalIgnoreCase))
            {
                $"Failed to cherry pick commit {hash.Error()} : {result.Error()}".WriteLine();
            }
            else
            {
                $"Successfully cherry picked commit {hash.Success()}".WriteLine();
            }
            await Task.Delay(50);
        }
    }
}