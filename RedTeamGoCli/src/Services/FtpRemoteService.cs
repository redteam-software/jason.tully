using FluentFTP;
using System.Net;
using System.Text;

namespace RedTeamGoCli.Services;

[RegisterScoped<IRemoteService>(Duplicate = DuplicateStrategy.Append, ServiceKey = "go-production-cf")]
[RegisterScoped<IRemoteService>(Duplicate = DuplicateStrategy.Append, ServiceKey = "go-manager")]
public class FtpRemoteService : IRemoteService
{
    private FtpClient? _client = null;
    private bool _disposedValue;

    public Task DeleteRemoteFile(string path, IGoRemoteServiceProject remoteProject, INotificationService notificationService)
    {
        //not implemented
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public Task<DownloadFileResult> DownloadRemoteFile(string remotePath, string destination, IGoRemoteServiceProject remoteProject, INotificationService notificationService, bool format = false)
    {
        return Task.FromResult(new DownloadFileResult("", ""));
    }

    public async Task<bool> Initialize(IGoRemoteServiceProject remoteProject, INotificationService notificationService)
    {
        var goFtpProject = remoteProject as IGoFtpProject;
        if (goFtpProject != null)
        {
            _client = new FtpClient();
            _client.Host = remoteProject.Host;
            var pwd = GetPassword(goFtpProject);
            _client.Credentials = new NetworkCredential(goFtpProject.User, pwd);

            await notificationService.NotifyAsync($"Connecting to FTP Server {remoteProject.Host}@{remoteProject.RemoteDirectory}");
            _client.Connect();

            await notificationService.NotifyAsync($"Connection Established".Success());
            return true;
        }
        return false;
    }

    public async Task<UploadStatus> UploadRemoteFile(SourceFile source, string destination, IGoRemoteServiceProject remoteProject, INotificationService notificationService)
    {
        if (_client == null)
        {
            return new UploadStatus(false, "FTP Client Not Initialized");
        }

        var correlationId = source.RelativePath.ToLower();

        await notificationService.NotifyAsync(new Notification(correlationId, $"Uploading {source.RelativePath.TextValue(true)}", "FTPUpload", DateTime.Now));
        try
        {
            var status = _client.UploadBytes(source.Content, destination,
                     FtpRemoteExists.OverwriteInPlace,
                     false);

            if (status != FtpStatus.Success)
            {
                await notificationService.NotifyAsync(new Notification(correlationId, "Failed To Upload File".Error(), "FTPUpload", DateTime.Now));
                return new UploadStatus(false, status.ToString());
            }
            else
            {
                await notificationService.NotifyAsync(new Notification(correlationId, $"File {source.RelativePath.Success()} Uploaded", "FileUpload", DateTime.Now));
                return new UploadStatus(true, "File Uploaded Successfully");
            }
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException;
            var sb = new StringBuilder();
            sb.AppendLine(ex.Message).AppendLine(inner?.Message);
            await notificationService.NotifyAsync($"{sb.ToString().Error()} for file {destination}");
            return new UploadStatus(false, ex.Message);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _client?.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    private string GetPassword(IGoFtpProject parameters)
    {
        try
        {
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(parameters.Password));
        }
        catch
        {
            return parameters.Password;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~FtpRemoteTarget()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }
}