namespace RedTeamGoCli.Services;
public class DefaultNotificationService : INotificationService
{
    public static INotificationService Instance { get; } = new DefaultNotificationService();
    public Task NotifyAsync(Notification notification, CancellationToken? cancellationToken = null)
    {
        $"{notification.Message}".WriteLine();
        return Task.CompletedTask;
    }
}
