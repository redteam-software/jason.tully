namespace RedTeamGoCli.Interfaces;

public record SourceFile(string LocalPath, string RelativePath, byte[] Content);
public record UploadStatus(bool Success, string Message);

public interface IRemoteService : IDisposable
{
    public Task DeleteRemoteFile(string remotePath, IGoRemoteServiceProject remoteProject, INotificationService notificationService);

    public Task DownloadRemoteFile(string remotePath, string destination, IGoRemoteServiceProject remoteProject, INotificationService notificationService);

    public Task<bool> Initialize(IGoRemoteServiceProject remoteProject, INotificationService notificationService);

    public Task<UploadStatus> UploadRemoteFile(SourceFile source, string destination, IGoRemoteServiceProject remoteProject, INotificationService notificationService);
}