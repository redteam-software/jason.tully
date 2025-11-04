namespace RedTeamGoCli.Interfaces;

public interface IRemoteChangeSynchronizationService
{
    public Task StartAsync(SynchronizationParameters parameters, INotificationService notificationService, CancellationToken cancellationToken);
}