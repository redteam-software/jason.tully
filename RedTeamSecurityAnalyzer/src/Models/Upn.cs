using Microsoft.Extensions.Configuration;

namespace RedTeamSecurityAnalyzer.Models;
public record Upn(string Username, string Password)
{
    public static Upn Create(string? Username, string? Password, IConfiguration configuration)
    {
        var username = Username ?? configuration["username"];
        var password = Password ?? configuration["password"];

        return new Upn(username!, password!);
    }
}

