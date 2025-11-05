namespace RedTeamGoCli.Interfaces;

public interface IGrafanaCloudService
{
    Task PostLokiLogMessageAsync(string logMessage, Dictionary<string, string>? tags = null);
}