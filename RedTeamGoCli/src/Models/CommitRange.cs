namespace RedTeamGoCli.Models;

public record CommitRange(string HashStart, string? HashEnd)
{
    public static CommitRange? GetCommitRange(string? hashStart, string? hashEnd)
    {
        if (!string.IsNullOrWhiteSpace(hashStart))
        {
            return new CommitRange(hashStart!, hashEnd!);
        }
        return null;
    }

    public override string ToString()
    {


        if (!string.IsNullOrWhiteSpace(HashStart))
        {

            if (!string.IsNullOrWhiteSpace(HashEnd))
            {
                return $"Commits From: {HashStart.Information()} To: {HashEnd.Information()}";
            }
            else
            {
                return $"Commits From: {HashStart.Information()}";
            }
        }
        return string.Empty;
    }
}