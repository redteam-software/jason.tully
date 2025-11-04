namespace RedTeamGoCli.Models;

public record GoColdFusionChangeSet(IReadOnlyList<Change> Changes, string RemoteDirectory = "mdrive\\paskrcustomers\\uatcode");

