


namespace RedTeamCommand.Commands;

public record RedTeamCommandParameters(
  string? logLevel = "information") : CommandParameters(logLevel);

[CommandHandler("_command_", "Describe your command here.")]
public class RedTeamCommandCommand : ICommand<RedTeamCommandParameters>
{
    public async Task RunAsync(RedTeamCommandParameters args, CommandContext commandContext, CancellationToken cancellationToken = default)
    {
        $"RedTeamCommand command executed with logLevel: {args.logLevel}".WriteLine();
    }

}



