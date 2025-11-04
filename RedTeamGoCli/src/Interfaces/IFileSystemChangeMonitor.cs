namespace RedTeamGoCli.Interfaces;

public interface IFileSystemChangeMonitor
{
    Task MonitorAsync(string directoryPath, INotificationService notificationService, CancellationToken cancellationToken);
}