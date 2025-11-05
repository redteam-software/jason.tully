namespace RedTeamGoCli.Commands;

public record UnifiedLoginSignupParameters(
  [Option("email", Description = "The user email address.")] string email,
    [Option(Description = "the application name. default is go.")] string applicationName = "go",
 string? logLevel = "error") : CommandParameters(logLevel);

/// <summary>
/// Sends a unified login invitation to a user's email address via the RedTeam Platform service.
/// Retrieves an access token from the platform and uses it to initiate the signup process
/// for the specified application (defaults to "go").
/// </summary>
[SubCommand(SubCommandUser.SubCommandName, SubCommandUser.SubCommandDescription)]
[SubCommandHandler(
    SubCommandUser.SubCommandName,
    SubCommandUser.CommandUnifiedLoginSignup.CommandName,
   SubCommandUser.CommandUnifiedLoginSignup.CommandDescription)]
public class UnifiedLoginSignupCommand : ICommand<UnifiedLoginSignupParameters>
{
    private readonly IRedTeamPlatformService _redTeamPlatformService;

    public UnifiedLoginSignupCommand(IRedTeamPlatformService redTeamPlatformService)
    {
        _redTeamPlatformService = redTeamPlatformService;
    }

    public async Task RunAsync(UnifiedLoginSignupParameters args, CommandContext commandContext, CancellationToken cancellationToken = default)
    {
        var accessToken = await _redTeamPlatformService.GetAccessTokenAsync();
        if (string.IsNullOrEmpty(accessToken))
        {
            "Failed to get access token".Error().WriteLine();
            return;
        }

        await _redTeamPlatformService.SignupAsync(accessToken!, args.email, args.applicationName);
    }
}