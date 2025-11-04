namespace RedTeamGit.Commands;

public record ListPullRequestsParameters(

     [Option(Common.Path, Description = Common.PathDescription)] string? path = null,
      string? logLevel = "none") : CommandParameters(logLevel);

[SubCommand(SubCommandGit.SubCommandName, SubCommandGit.SubCommandDescription)]
[SubCommandHandler(
   SubCommandGit.SubCommandName,
   SubCommandGit.CommandListPullRequests.CommandName,
   SubCommandGit.CommandListPullRequests.CommandDescription)]
public class ListPullRequestsCommand : ICommand<ListPullRequestsParameters>
{
    private readonly IGitHubCli _gitHubCli;
    private readonly IEnumerable<IGoProject> _goProjects;

    public ListPullRequestsCommand(IGitHubCli gitHubCli, IEnumerable<IGoProject> goProjects)
    {
        _gitHubCli = gitHubCli;
        _goProjects = goProjects;
    }


    public async Task RunAsync(ListPullRequestsParameters args, CommandContext commandContext, CancellationToken cancellationToken = default)
    {

        $"Getting Pull Requests for Go Projects {_goProjects.Count().NumericValue()}".WriteLine();

        if (!string.IsNullOrEmpty(args.path) && Directory.Exists(args.path))
        {
            Environment.CurrentDirectory = args.path;
        }


        $"Searching {Environment.CurrentDirectory} for go projects".WriteLine();

        var directory = new DirectoryInfo(Environment.CurrentDirectory);

        var projectDirectories = directory.GetDirectories("*", SearchOption.AllDirectories)
            .Where(d => _goProjects.Any(p => p.ProjectDirectory == d.Name));

        var query = from d in directory.GetDirectories("*", SearchOption.AllDirectories)
                    join p in _goProjects on d.Name equals p.ProjectDirectory
                    select new
                    {
                        Directory = d,
                        Project = p
                    };

        var data = new HashSet<PullRequest>();

        foreach (var item in query)
        {
            $"Getting Pull Requests for Project {item.Project.Name.TextValue(true)} in Directory {item.Directory.FullName.TextValue(true)}".WriteLine();
            var result = await _gitHubCli.ListPullRequests(item.Project.Name, item.Directory.FullName);

            if (result.Status && result.Data?.Any() == true)
            {
                foreach (var d in result.Data)
                {
                    data.Add(d);
                }
            }
        }


        var table = new Table().Expand().Border(TableBorder.Rounded);


        table.AddColumns("Project", "Title", "State", "Age", "URL");

        foreach (var item in data.OrderBy(x => x.project))
        {
            var message = item.createdAt;
            if (DateTime.TryParse(item.createdAt, out var createdAt))
            {
                message = $"{DateTime.UtcNow.Subtract(createdAt).TotalDays.NumericValue()} days";
            }
            table.AddRow(
                item.project!.NumericValue(),
                item.title.TextValue(false),
                item.state.TextValue(false),
                message,
                item.url.TextValue(true)
                );
        }

        AnsiConsole.Write(table);

    }


}