namespace RedTeamGoCli.Models;

public record DateRange(string Since, string? Until = null)
{
    public static DateRange? GetDateRange(string? since, string? until)
    {
        if (!string.IsNullOrWhiteSpace(since))
        {
            return new DateRange(since, until);
        }
        return null;
    }
}