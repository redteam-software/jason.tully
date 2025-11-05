namespace RedTeamGoCli.Interfaces;

public interface IGitHubCli
{
    public Task<GitHubCliResult> CreatePullRequest(string targetBranch, string sourceBranch, bool autoMerge);

    public Task<GitHubCliResult> MergePullRequest();

    public Task<GitHubCliResult> MonitorDeploymentAsync(ProgressTask task);

    public Task<GitHubCliResult<PullRequest>> ListPullRequests(string projectName, string localRepoPath);
}