namespace RedTeamGoCli.Models;

public record PullRequest(
    string id,
    string title, string state, string updatedAt, string createdAt, string url, string? project = null);
public record GitHubCliResult(bool Status, string? Message = null)
{
    public static GitHubCliResult Success => new GitHubCliResult(true, null);
    public static GitHubCliResult Error(string message) => new GitHubCliResult(true, message);
}

public record GitHubCliResult<T>(bool Status, List<T>? Data = null)
{
}