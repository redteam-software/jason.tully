using RedTeam.Extensions.Console.Requests;
using RedTeam.Extensions.Console.ShellInterop;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace RedTeamGit.Services;

[RegisterSingleton<IGitService>]
internal class GitCommandLineService : IGitService
{
    private static readonly string _regExPattern = @"^commit=(?<commit>[a-f0-9]{40})\|author=(?<author>[^|]+)\|email=(?<email>[^|]+)\|date=(?<date>[\d\-: ]+ [\+\-]\d{4})\|message=(?<message>.+)\|branch=(?<branch>.*)$";
    private static readonly Regex _groupingKey = new(_regExPattern, RegexOptions.Compiled);
    private readonly IShell _shell;

    public GitCommandLineService(IShell shell)
    {
        _shell = shell;
    }

    public string CherryPickCommit(string commitHash, bool noCommit = true)
    {
        var result = _shell.Invoke($"git cherry-pick {commitHash} --no-commit");
        if (result.Result?.Any(r => r.Contains("conflict", StringComparison.OrdinalIgnoreCase)) == true)
        {
            // Console.WriteLine($"Conflict detected on {commitInfo.Hash}. Overwriting with incoming changes...");
            //Force checkout of incoming version for all conflicted files
            _shell.Invoke("git checkout --theirs .");
            _shell.Invoke("git add .");
            _shell.Invoke("git cherry-pick --continue");
            return "Applied with Conflicts Resolved".Warning();
        }
        else
        {
            return "Applied Successfully".Success();
        }
    }

    public string? GetCurrentBranchName()
    {
        var response = _shell.InvokeCommand(new ShellCommand("git", null, "rev-parse", "--abbrev-ref", "HEAD"));

        if (response.StandardOutput != null)
        {
            using var stringReader = new StringReader(response.StandardOutput);
            var line = stringReader.ReadLine();
            return line?.Trim();
        }

        return null;
    }

    public List<Change> GetCurrentChanges()
    {
        //git diff-tree --no-commit-id --name-only -r HEAD
        var response = _shell.InvokeCommand(new ShellCommand("git", null, "diff-tree --no-commit-id --name-only -r HEAD"));
        var changes = new HashSet<Change>();

        var baseDirectory = new DirectoryInfo(Environment.CurrentDirectory);
        if (response.StandardOutput != null)
        {
            using var stringReader = new StringReader(response.StandardOutput);
            string? line = null;

            while ((line = stringReader.ReadLine()) != null)
            {
                line = line.Trim();
                var path = line.Replace("/", "\\");

                var fullPath = Path.Combine(baseDirectory.FullName, path);
                if (!File.Exists(fullPath))
                {
                    continue;
                }
                changes.Add(new Change(path, Path.Combine(baseDirectory.FullName, path)));
            }
        }
        return changes.ToList();
    }

    public IReadOnlyList<CommitInfo> SearchCommitsByAuthor(string author,
        CommitRange? commitRange,
        DateRange? dateRange,
        GitLogFormatString? gitLogFormatString,
        GitLogSearchFlags gitLogSearchFlags)
    {
        if (string.IsNullOrWhiteSpace(author))
        {
            throw new ArgumentException("Author cannot be null or whitespace.", nameof(author));
        }
        var formatString = gitLogFormatString ?? GitLogFormatString.CommitInfo;

        bool usePowershell = false;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            usePowershell = true;
        }

        var (fileNameOnly, removeDuplicates, restrictToCurrentBranch, searchAccrossAllBranches) = gitLogSearchFlags;

        List<ShellCommandArgument> args = [
            "log",
            "--no-merges",
            "--reverse",
            $"--pretty=format:{formatString.Value}",

            "--decorate",
            $"--committer=\"{author}\""
            ];

        if (fileNameOnly)
        {
            args.Insert(1, "--name-only");
        }

        if (searchAccrossAllBranches)
        {
            args.Insert(1, "--all");
        }

        if (dateRange != null)
        {
            args.Add($"--since=\"{dateRange.Since}\"");
            if (!string.IsNullOrWhiteSpace(dateRange.Until))
            {
                args.Add($"--until=\"{dateRange.Until}\"");
            }
        }

        if (commitRange != null)
        {
            var (start, end) = commitRange;

            if (string.IsNullOrEmpty(end))
            {
                args.Add($"{start}^");
            }
            else
            {
                string rangeFilter = $"{start}^..{end}";
                args.Add(rangeFilter);
            }
        }

        var settings = new ShellCommandOptions(StringChannel.Unbounded(), StringChannel.Unbounded(), UsePowershell: usePowershell);

        var command = new ShellCommand("git", settings, args.ToArray());

        var result = _shell.InvokeCommand(command);

        if (!result.CompletedSuccessfully)
        {
            return Array.Empty<CommitInfo>();
        }

        using var sr = new StringReader(result.StandardOutput!);
        string? line = null;
        List<CommitInfo> commitInfos = new();
        Stack<CommitInfo> commitStack = new();
        while ((line = sr.ReadLine()) != null)
        {
            // Process each line as needed
            var match = _groupingKey.Match(line);

            if (match.Success)
            {
                var commitHash = match.Groups["commit"].Value;
                if (commitStack.Any())
                {
                    //already in stack?
                    var existing = commitStack.Peek();
                    if (existing.Hash == commitHash)
                    {
                        continue;
                    }

                    var finishedCommit = commitStack.Pop();
                    commitInfos.Add(finishedCommit);
                }

                var authorName = match.Groups["author"].Value;
                var email = match.Groups["email"].Value;
                var date = match.Groups["date"].Value;
                var message = match.Groups["message"].Value;
                var branch = match.Groups["branch"].Value;

                DateTime? pd = null;
                if (DateTime.TryParse(date, out var parsedDate))
                {
                    pd = parsedDate;
                }
                var commitInfo = new CommitInfo(message, commitHash, authorName, email, pd, new HashSet<string>(), branch);
                commitStack.Push(commitInfo);
            }
            else
            {
                //not a commit line, must be a changed file if we're in fileNameOnly mode.
                if (commitStack.Any())
                {
                    var currentCommit = commitStack.Peek();
                    currentCommit.ChangedFiles?.Add(line);
                }
            }
        }

        //var commits = result.Result!.Select(CommitInfo.Parse).ToList();

        if (commitStack.Any())
        {
            var finishedCommit = commitStack.Pop();
            commitInfos.Add(finishedCommit);
        }

        return commitInfos;
    }
}