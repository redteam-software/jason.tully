using Spectre.Console.Rendering;
using System.Text;

namespace RedTeamGit.Commands;

public record ListChangesParameters(
      [Option(Common.Author, Description = Common.AuthorDescription)] string author,
      [Option(Common.HashStart, Description = Common.HashStartDescription)] string? commitStart = null,
      [Option(Common.HashEnd, Description = Common.HashEndDescription)] string? commitEnd = null,
      [Option(Common.Since, Description = Common.SinceDescription)] string? since = null,
      [Option(Common.Until, Description = Common.UntilDescription)] string? until = null,
      [Option(Common.Path, Description = Common.PathDescription)] string? path = null,
      [Option(Common.AllBranches, Description = Common.AllBranchesDescription)] bool? all = null,
      [Option("output", Description = "the output display of data.  [fileName|brief|full")] string? output = "fileName",
        string? logLevel = "none") : CommandParameters(logLevel);

/// <summary>
/// Generates a comprehensive list of file changes made by a specific author within a commit or date range.
/// Supports multiple output formats: fileName (paths only), brief (commit table), or full (tree view with details).
/// Provides statistics on distinct and total file changes. Does not include actual diffs, only file names.
/// </summary>
[SubCommand(SubCommandGit.SubCommandName, SubCommandGit.SubCommandDescription)]
[SubCommandHandler(
   SubCommandGit.SubCommandName,
   SubCommandGit.CommandListChanges.CommandName,
   SubCommandGit.CommandListChanges.CommandDescription)]
public class ListChangesCommand : ICommand<ListChangesParameters>
{
    private readonly IGitService _gitService;

    public ListChangesCommand(IGitService gitService)
    {
        _gitService = gitService;
    }

    public async Task RunAsync(ListChangesParameters args, CommandContext commandContext, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(args.path) && Directory.Exists(args.path))
        {
            Environment.CurrentDirectory = args.path;
        }

        var dateRange = DateRange.GetDateRange(args.since, args.until);
        CommitRange? commitRange = CommitRange.GetCommitRange(args.commitStart, args.commitEnd);

        var message = new StringBuilder($"""
        Listing Changes
        Author: {args.author.Information()}
        Project: {Environment.CurrentDirectory.TextValue(true)}
        {commitRange?.ToString() ?? ""}
        Generates a list of changes filtered by commit ranges and or author.  This will only yield the names of the files changed not the actual diff.
        """);

        message.ToString().WriteLine();

        var commitsByAuthor = _gitService.SearchCommitsByAuthor(
            args.author,
            commitRange,
            dateRange,
            GitLogFormatString.CommitInfo,
            new GitLogSearchFlags(true, true, true, args.all ?? false));

        var table = new Table().Expand();
        IRenderable renderable = table;
        var display = args.output ?? "fileName";

        if (display == "fileName")
        {
            table.AddColumn("File");

            var filePaths = commitsByAuthor
        .SelectMany(c => c.ChangedFiles!)
        .Where(m => !string.IsNullOrWhiteSpace(Path.GetExtension(m)) && IsValidPath(m))
        .Distinct()
        .OrderBy(x => x);

            foreach (var filePath in filePaths)
            {
                table.AddRow($"{Path.Combine(Environment.CurrentDirectory, filePath).TextValue(true)}");
            }
        }
        else if (display == "brief")
        {
            table.AddColumn("Commit Hash", c =>
            {
                c.Width = 64;
            });
            table.AddColumn("Branch", c =>
            {
            });
            table.AddColumn("Date");

            foreach (var commit in commitsByAuthor)
            {
                table.AddRow($"{commit.Hash!.Success()}", $"{commit.Branch}", $"{commit.Date.DateTimeValue()}");
            }
        }
        else if (display == "full")
        {
            var tree = new Tree("Commit Overview");
            renderable = tree;

            //table.Expand();
            //table.AddColumn("Commit Hash");
            //table.AddColumn("Author");
            //table.AddColumn("Date");
            //table.AddColumn("Change Count");

            foreach (var commit in commitsByAuthor)
            {
                var node = tree.AddNode($"({commit.Date}) - {commit.Hash} - {commit.Author} - {commit.Branch}");

                var titleNode = node.AddNode($"{commit.Message.Colorize(ThemeColors.Title, new TextDecorations(true, true, false, false))}");
                var filePaths = commitsByAuthor
                        .SelectMany(c => commit.ChangedFiles!)
                        .Where(m => !string.IsNullOrWhiteSpace(Path.GetExtension(m)) && IsValidPath(m))
                        .Distinct()
                        .OrderBy(x => x);

                var rows = new Rows(filePaths.Select(x => new Markup(Path.Combine(Environment.CurrentDirectory, x).TextValue(true))));
                titleNode.AddNode(rows);
            }
        }

        var distinctChanges = commitsByAuthor
        .SelectMany(c => c.ChangedFiles!)
        .Where(m => !string.IsNullOrWhiteSpace(Path.GetExtension(m)) && IsValidPath(m))
        .Distinct().Count();

        var totalChanges = commitsByAuthor
       .SelectMany(c => c.ChangedFiles!)
       .Where(m => !string.IsNullOrWhiteSpace(Path.GetExtension(m)) && IsValidPath(m)).Count();

        AnsiConsole.Write(renderable);
        $"Distinct File Changes: {distinctChanges.NumericValue()}".WriteLine();
        $"Total Changes: {totalChanges.NumericValue()}".WriteLine();

        await Task.Delay(100);
    }

    private static bool IsValidPath(string path)
    {         // Check for invalid characters
        return path.Contains("\\") || path.Contains("/");
    }
}