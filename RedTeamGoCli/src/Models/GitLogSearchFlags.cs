namespace RedTeamGoCli.Models;
public record GitLogSearchFlags(bool FileNameOnly, bool RemoveDuplicates, bool RestrictToCurrentBranch, bool SearchAcrossAllBranches);
