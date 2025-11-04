namespace RedTeamGoCli.Interfaces;

public interface IGitService
{
    public string CherryPickCommit(string commitHash, bool noCommit = true);

    public string? GetCurrentBranchName();

    public List<Change> GetCurrentChanges();
    public IReadOnlyList<CommitInfo> SearchCommitsByAuthor(
        string author,
        CommitRange? commitRange,
        DateRange? dateRange,
        GitLogFormatString? gitLogFormatString,
        GitLogSearchFlags gitLogSearchFlags);
}
