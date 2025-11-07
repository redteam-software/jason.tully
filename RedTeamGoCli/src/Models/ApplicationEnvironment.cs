namespace RedTeamGoCli.Models;
public record ApplicationEnvironment(string Value)
{
    private const string _defaultEnvironment = "uat";
    public static implicit operator string(ApplicationEnvironment environment) => environment.Value;
    public static implicit operator ApplicationEnvironment(string? environment) => new ApplicationEnvironment(environment ?? _defaultEnvironment);
}
