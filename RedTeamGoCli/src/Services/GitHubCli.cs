using RedTeam.Extensions.Console.ShellInterop;
using System.Text.Json;

namespace RedTeamGoCli.Services;

[RegisterSingleton<IGitHubCli>]
public class GitHubCli : IGitHubCli
{
    private readonly IShell _shell;

    public GitHubCli(IShell shell)
    {
        _shell = shell;
    }

    public async Task<GitHubCliResult> CreatePullRequest(string targetBranch, string sourceBranch, bool autoMerge)
    {
        //gh pr create --base uatcode_v3 --head feature/RED_5073_UnifiedLogin --fill

        var prResponse = _shell.Invoke($"gh pr create --base {targetBranch} --head {sourceBranch} --fill");

        if (prResponse.ExitCode == 0)
        {
            if (autoMerge)
            {
                return await MergePullRequest();
            }
            return new GitHubCliResult(true, prResponse.Result!.First());
        }
        else
        {
            return GitHubCliResult.Error($"Failed to create PR from {sourceBranch} to {targetBranch}");
        }
    }

    public string? GetCurrentUser()
    {
        var response = _shell.InvokeCommand(new ShellCommand("gh", null, "api", "/user", "--jq", ".login"));
        if (response.StandardOutput != null)
        {
            return response.StandardOutput.Trim();
        }
        return null;
    }

    public async Task<GitHubCliResult<PullRequest>> ListPullRequests(string projectName, string localRepoPath)
    {
        var current = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = localRepoPath;
            var results = _shell.Invoke("gh pr list --author \"@me\" --json \"id,title,state,updatedAt,createdAt,url\"");

            if (results.CompletedSuccessfully && results.Result?.Any() == true)
            {
                var prs = new List<PullRequest>();
                foreach (var r in results.Result)
                {
                    try
                    {
                        var pr = JsonSerializer.Deserialize<List<PullRequest>>(r);
                        if (pr != null)
                        {
                            prs.AddRange(pr.Select(x => x with { project = projectName }));
                        }
                    }
                    catch (Exception)
                    {
                        //ignore
                    }
                }

                return new GitHubCliResult<PullRequest>(true, prs);
            }

            return new GitHubCliResult<PullRequest>(results.ExitCode == 0);
        }
        finally
        {
            Environment.CurrentDirectory = current;
        }
    }

    public async Task<GitHubCliResult> MergePullRequest()
    {
        try
        {
            await Task.Delay(1500); // Wait for a moment to ensure the PR is created before attempting to merge
                                    //gh pr merge --auto --merge
            var mergeResponse = _shell.InvokeCommand(new ShellCommand("gh", null, "pr", "merge", "--auto", "--merge"));

            if (mergeResponse.ExitCode == 0)
            {
                return GitHubCliResult.Success; ;
            }
            else
            {
                return GitHubCliResult.Error("Failed to set PR to auto merge");
            }
        }
        catch (Exception ex)
        {
            return GitHubCliResult.Error($"Error during auto-merge process: {ex.Message}");
        }
    }

    public async Task<GitHubCliResult> MonitorDeploymentAsync(ProgressTask task)
    {
        try
        {
            var isSuccess = false;
            var token = new CancellationTokenSource(TimeSpan.FromMinutes(2));
            task.PostTaskMessage(new RedTeam.Extensions.Console.Models.ProgressTaskMessage($"Waiting For Action to Start", 200, ""));
            await Task.Delay(3000);
            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var listResponse = _shell.InvokeCommand(new ShellCommand("gh", null, "run", "list", "--json", "headBranch,event,databaseId,displayTitle,status,conclusion,workflowName"));
                        List<StatusModel> result = new List<StatusModel>();
                        string? line = null;
                        if (listResponse.StandardOutput == null)
                        {
                            continue;
                        }
                        using var sr = new StringReader(listResponse.StandardOutput);
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line.StartsWith("[") && line.EndsWith("]"))
                            {
                                var results = JsonSerializer.Deserialize<List<StatusModel>>(line);
                                if (results != null)
                                {
                                    result.AddRange(results);
                                }
                            }
                        }

                        if (result.All(x => x.status == "completed"))
                        {
                            task.PostTaskMessage(new RedTeam.Extensions.Console.Models.ProgressTaskMessage($"Run Completed.", 200, ""));
                            token.Cancel();
                            isSuccess = true;
                        }
                        else
                        {
                            task.PostTaskMessage(new RedTeam.Extensions.Console.Models.ProgressTaskMessage($"Last Check {DateTime.Now}", 200, ""));
                            await Task.Delay(1000);
                        }
                    }
                    catch (Exception ex)
                    {
                        task.PostTaskMessage(new RedTeam.Extensions.Console.Models.ProgressTaskMessage($"Error checking action status: {ex.Message}", 500, ""));
                        await Task.Delay(1000);
                        isSuccess = false;
                    }
                }
            }
            catch (Exception)
            {
                isSuccess = false;
            }

            return new GitHubCliResult(isSuccess);
        }
        catch (Exception ex)
        {
            return GitHubCliResult.Error($"Error during auto-merge process: {ex.Message}");
        }
    }

    internal class StatusModel
    {
        public string? conclusion { get; set; }
        public long databaseId { get; set; }
        public string? displayTitle { get; set; }
        public string? headBranch { get; set; }
        public string? status { get; set; }
        public string? workflowName { get; set; }
    }
}