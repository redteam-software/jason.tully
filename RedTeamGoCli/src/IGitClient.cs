using RedTeam.Extensions.Console.ShellInterop;

namespace RedTeamGoCli;

public record Change(string RelativePath, string FullPath);
public interface IGitClient
{
    public string? GetCurrentBranchName();

    public List<Change> GetCurrentChanges();
}

[RegisterSingleton<IGitClient>]
public class CommandLineGitClient : IGitClient
{
    private readonly IShell _shell;

    public CommandLineGitClient(IShell shell)
    {
        _shell = shell;
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
}
