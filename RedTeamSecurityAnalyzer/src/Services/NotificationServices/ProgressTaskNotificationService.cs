using Microsoft.Extensions.Logging;
using RedTeam.Extensions.Console.ExtensionMethods;
using RedTeam.Extensions.Console.Logging;
using RedTeam.Extensions.Console.Models;
using Spectre.Console;

namespace RedTeamSecurityAnalyzer.Services.NotificationServices;

internal class ProgressTaskNotificationService : IProgressTaskNotificationService
{
    private readonly ILogger _logger;

    public ProgressTask ProgressTask { get; init; }

    public ProgressTaskNotificationService(ProgressTask progressTask, ILoggerFactory loggerFactory)
    {
        ProgressTask = progressTask;
        _logger = loggerFactory.CreateLogger<ProgressTaskNotificationService>();
    }

    public Task NotifyAsync(ProgressTaskMessage message)
    {

        ProgressTask.PostTaskMessage(message);
        return Task.CompletedTask;
    }

    public Task NotifyAsync(Dictionary<string, object> state)
    {
        if (state.ContainsKey("Message"))
        {

            ProgressTask.PostTaskMessage(new ProgressTaskMessage(state["Message"].ToString()!, 0, null));
        }
        return Task.CompletedTask;
    }

    public Task NotifyAsync(AggregatedProgressTaskMessage message)
    {
        ProgressTask.PostTaskMessage(message.ProgressTaskMessage);

        ProgressTask.State.Update<SecurityAnalysisStatusMessage>(nameof(SecurityAnalysisStatusMessage), key => message.SecurityAnalysisStatusMessag);
        return Task.CompletedTask;
    }

    public void Exception(Exception ex)
    {
        _logger.LogError(ex, "An error occurred in the ProgressTaskNotificationService {message}", ex.Message);
    }

    public void Information(string message)
    {
        _logger.LogInformation(message);
    }
}