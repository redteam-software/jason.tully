namespace RedTeamSecurityAnalyzer.Models;

/// <summary>
/// Available Red Team applications.
/// </summary>
public enum RedTeamApplication
{
    Go,
    Flex,
    Lens,
}

public static class RedTeamApplicationUtil
{
    /// <summary>
    /// Converts a string to a RedTeamApplication enum value.  The default is Go.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
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