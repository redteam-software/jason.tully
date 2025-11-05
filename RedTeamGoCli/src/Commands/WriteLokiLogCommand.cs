namespace RedTeamGoCli.Commands;

public record WriteLokiLogParameters(

      [Option("message", Description = "message to log")] string message,
      [Option("tags", Description = "tags to include with the log message.  A comma delimited list of key=value pairs.")] string? tags = null,
      string? logLevel = "none") : CommandParameters(logLevel);

/// <summary>
/// Sends log messages to Grafana Cloud Loki for centralized logging and monitoring.
/// Supports optional tags as key-value pairs (comma-delimited) for log categorization and filtering.
/// Integrates with Grafana Cloud service for real-time log aggregation.
/// </summary>
[SubCommand(SubCommandLogs.SubCommandName, SubCommandLogs.SubCommandDescription)]
[SubCommandHandler(
   SubCommandLogs.SubCommandName,
   SubCommandLogs.CommandWriteLokiLog.CommandName,
   SubCommandLogs.CommandWriteLokiLog.CommandDescription)]
public class WriteLokiLogCommand : ICommand<WriteLokiLogParameters>
{
    private readonly IGrafanaCloudService _grafanaCloudService;

    public WriteLokiLogCommand(IGrafanaCloudService grafanaCloudService)
    {
        _grafanaCloudService = grafanaCloudService;
    }

    public async Task RunAsync(WriteLokiLogParameters args, CommandContext commandContext, CancellationToken cancellationToken = default)
    {
        Dictionary<string, string> logTags = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(args.tags))
        {
            var tags = args.tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var tag in tags)
            {
                var keyValue = tag.Split('=', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (keyValue.Length == 2)
                {
                    logTags[keyValue[0]] = keyValue[1];
                }
            }
        }
        await _grafanaCloudService.PostLokiLogMessageAsync(args.message, logTags);
    }
}