using RedTeam.Extensions.Console.ExtensionMethods;
using RedTeam.Extensions.Console.ShellInterop;
using System.Text.Json;

namespace RedTeamGoCli;
public interface IGitHubCli
{
    public Task<Result> CreatePullRequest(string targetBranch, string sourceBranch, bool autoMerge);
    public Task<Result> MergePullRequest();

    public Task<Result> MonitorDeploymentAsync(ProgressTask task);
}
public record Result(bool Status, string? Message = null)
{
    public static Result Success => new Result(true, null);
    public static Result Error(string message) => new Result(true, message);
}

[RegisterSingleton<IGitHubCli>]
public class GitHubCli : IGitHubCli
{
    private readonly IShell _shell;
    public GitHubCli(IShell shell)
    {
        _shell = shell;
    }

    public async Task<Result> CreatePullRequest(string targetBranch, string sourceBranch, bool autoMerge)
    {
        //gh pr create --base uatcode_v3 --head feature/RED_5073_UnifiedLogin --fill


        var prResponse = _shell.InvokeCommand(new ShellCommand("gh", null, "pr", "create", "--base", targetBranch, "--head", sourceBranch, "--fill"));

        if (prResponse.ExitCode == 0)
        {

            if (autoMerge)
            {
                return await MergePullRequest();
            }
            return Result.Success;
        }
        else
        {

            return Result.Error($"Failed to create PR from {sourceBranch} to {targetBranch}");
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

    public async Task<Result> MergePullRequest()
    {
        try
        {
            await Task.Delay(1500); // Wait for a moment to ensure the PR is created before attempting to merge
                                    //gh pr merge --auto --merge  
            var mergeResponse = _shell.InvokeCommand(new ShellCommand("gh", null, "pr", "merge", "--auto", "--merge"));

            if (mergeResponse.ExitCode == 0)
            {
                return Result.Success; ;
            }
            else
            {

                return Result.Error("Failed to set PR to auto merge");
            }
        }
        catch (Exception ex)
        {

            return Result.Error($"Error during auto-merge process: {ex.Message}");
        }
    }

    public async Task<Result> MonitorDeploymentAsync(ProgressTask task)
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

            return new Result(isSuccess);
        }
        catch (Exception ex)
        {

            return Result.Error($"Error during auto-merge process: {ex.Message}");
        }
    }

    public class StatusModel
    {
        public string? conclusion { get; set; }
        public long databaseId { get; set; }
        public string? displayTitle { get; set; }
        public string? headBranch { get; set; }
        public string? status { get; set; }
        public string? workflowName { get; set; }
    }

}
