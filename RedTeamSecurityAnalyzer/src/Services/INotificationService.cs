using Spectre.Console;
using Spectre.Console.Rendering;

namespace RedTeamSecurityAnalyzer.Services;

public interface INotificationService
{
    public Task NotifyAsync(Dictionary<string, object> state);

    public void Exception(Exception ex);

    public void Information(string message);
}

public interface INotificationService<TMessage> : INotificationService
{
    public Task NotifyAsync(TMessage message);
}

public interface ILiveDisplayNotificationService : INotificationService<IRenderable>
{
    public LiveDisplayContext LiveDisplayContext { get; }
}

public interface IProgressTaskNotificationService : INotificationService<AggregatedProgressTaskMessage>
{
    public ProgressTask ProgressTask { get; }
}

public static class NotificationServiceExtensions
{
    public static Task NotifyAsync(this IProgressTaskNotificationService notificationService, string message)
    {
        return notificationService.NotifyAsync(new(new(message, 0, default), new()));
    }

    public static Task NotifyAsync(this IProgressTaskNotificationService notificationService, string message, int failureCount, int passedCount)
    {
        return notificationService.NotifyAsync(new(new(message, 0, default), new(failureCount > 0 ? SecurityAnalysisStatus.Failed : SecurityAnalysisStatus.Passed, failureCount, passedCount)));
    }

    public static Task NotifyAsync(this INotificationService notificationService, string message)
    {
        return notificationService.NotifyAsync(new Dictionary<string, object> { { "Message", message } });
    }

    internal class DefaultNotificationService : INotificationService
    {
        public void Exception(Exception ex)
        {

        }

        public void Information(string message)
        {

        }

        public Task NotifyAsync(Dictionary<string, object> state)
        {
            return Task.CompletedTask;
        }
    }

    public static class EmptyNotificationService
    {
        public static INotificationService Default => new DefaultNotificationService();
    }
}