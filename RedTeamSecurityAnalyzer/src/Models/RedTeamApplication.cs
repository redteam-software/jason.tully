namespace RedTeamSecurityAnalyzer.Models;
public enum RedTeamApplication
{
    Go,
    Flex,
    Lens,
}

public static class RedTeamApplicationUtil
{
    public static RedTeamApplication Parse(string name)
    {
        return name.ToLower() switch
        {
            "go" => RedTeamApplication.Go,
            "flex" => RedTeamApplication.Flex,
            "lens" => RedTeamApplication.Lens,
            _ => RedTeamApplication.Go
        };
    }
}
