

namespace RedTeamCommandApp.Commands;

public record DefaultCommandNameParameters(
  string? logLevel = "information") : CommandParameters(logLevel);

[CommandHandler("DefaultCommandNameName", "Describe your command here.")]
public class DefaultCommandNameCommand : ICommand<DefaultCommandNameParameters>
{
    public async Task RunAsync(DefaultCommandNameParameters args, CommandContext commandContext, CancellationToken cancellationToken = default)
    {
        $"DefaultCommandName command executed with logLevel: {args.logLevel}".WriteLine();
    }

}



