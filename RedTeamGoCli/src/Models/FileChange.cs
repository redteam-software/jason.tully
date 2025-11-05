namespace RedTeamGoCli.Models;

public record FileChange(string FilePath, ChageType ChageType, DateTime EventTime, string? OldPath = null);