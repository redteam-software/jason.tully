namespace RedTeamGoCli.Interfaces;

public interface INotificationService
{
    public Task NotifyAsync(Notification notification, CancellationToken? cancellationToken = default);

}

public static class INotficationServiceExtensions
{
    public static Task NotifyAsync(this INotificationService service, string message)
    {
        return service.NotifyAsync(new Notification(NotificationId.System, message, "System".Warning(), DateTime.Now));
    }
}
