namespace RedTeamGoCli.Commands;
public record BaseCommandParameters(
     [Option(Common.Path, Description = Common.PathDescription)] string? path = null,
     [Option(Common.Environment, Description = Common.EnvironmentDescription)] string? env = null,
    string? logLevel = "error") : CommandParameters(logLevel);

