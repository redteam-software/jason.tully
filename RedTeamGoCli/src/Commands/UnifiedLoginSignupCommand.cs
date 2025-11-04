namespace RedTeamGoCli.Commands;

public record UnifiedLoginSignupParameters(
  [Option("email", Description = "The user email address.")] string email,
    [Option(Description = "the application name. default is go.")] string applicationName = "go",
 string? logLevel = "error") : CommandParameters(logLevel);

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



