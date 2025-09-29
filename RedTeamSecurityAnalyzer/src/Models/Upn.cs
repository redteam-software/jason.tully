using Microsoft.Extensions.Configuration;

namespace RedTeamSecurityAnalyzer.Models;
/// <summary>
///  encapsulates a username and password combination, typically used for authentication purposes.
/// </summary>
/// <param name="Username"></param>
/// <param name="Password"></param>
public record Upn(string Username, string Password)
{
    /// <summary>
    /// creates an instance of the Upn record using provided parameters or configuration values.
    /// </summary>
    /// <param name="Username"></param>
    /// <param name="Password"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static Upn Create(string? Username, string? Password, IConfiguration configuration)
    {
        var username = Username ?? configuration["username"];
        var password = Password ?? configuration["password"];

        return new Upn(username!, password!);
    }
}

