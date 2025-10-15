using FluentFTP;
using System.Net;
using System.Text;
using System.Threading.Channels;

namespace RedTeamGoCli.Services;

public record SynchronizationParameters(string User, string Password,

    int BatchSize = 5, int DebounceSeconds = 10,
     string? Host = null,
    string? RemoteDirectoryPath = null);
public interface IRemoteChangeSynchronizationService
{
    public Task StartAsync(SynchronizationParameters parameters, INotificationService notificationService, CancellationToken cancellationToken);
}

[RegisterSingleton<IRemoteChangeSynchronizationService>]
public class RemoteChangeSynchronizationService : IRemoteChangeSynchronizationService
{
    private const string _defaultHost = "10.0.0.4";
    private const string _remoteDirectory = "mdrive\\paskrcustomers\\uatcode";
    private readonly FileChangeChannel _fileChangeChannel;


    string GetPassword(SynchronizationParameters parameters)
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


    public RemoteChangeSynchronizationService(FileChangeChannel fileChangeChannel)
    {
        _fileChangeChannel = fileChangeChannel;
    }
    public async Task StartAsync(SynchronizationParameters parameters, INotificationService notificationService, CancellationToken cancellationToken)
    {
        var host = string.IsNullOrEmpty(parameters.Host) ? _defaultHost : parameters.Host;
        var remoteDir = string.IsNullOrEmpty(parameters.RemoteDirectoryPath) ? _remoteDirectory : parameters.RemoteDirectoryPath;

        var client = new FtpClient();
        client.Host = host;
        var pwd = GetPassword(parameters);
        client.Credentials = new NetworkCredential(parameters.User, pwd);
        await notificationService.NotifyAsync($"Connecting to FTP Server {parameters.Host}");
        client.Connect();
        await notificationService.NotifyAsync($"Connection Established".Success());

        var remoteDirectory = remoteDir;

        await notificationService.NotifyAsync(new Notification(Guid.NewGuid(), $"Batch Size:{parameters.BatchSize.NumericValue()}.  Debounce (secs): {parameters.DebounceSeconds.NumericValue()}"));

        await foreach (var batch in _fileChangeChannel.Reader.ReadAllBatches(parameters.BatchSize, TimeSpan.FromSeconds(parameters.DebounceSeconds), cancellationToken))
        {

            foreach (var changeSet in batch.GroupBy(x => x.FilePath))
            {
                var latest = changeSet.OrderByDescending(x => x.EventTime).First();

                if (latest.ChageType == ChageType.Delete)
                {

                }
                else if (File.Exists(latest.FilePath))
                {
                    var relative = latest.FilePath.Replace(Environment.CurrentDirectory, string.Empty);

                    var correlationId = latest.FilePath.ToLower();
                    var remotePath = remoteDirectory + relative;
                    await notificationService.NotifyAsync(new Notification(correlationId, $"Uploading {remotePath.TextValue(true)}", "FTPUpload".Success(), DateTime.Now));
                    try
                    {
                        var content = await ReadFileWithRetryAsync(latest.FilePath, 3, 200, cancellationToken);
                        var status = client.UploadBytes(content, remotePath,
                                 FtpRemoteExists.OverwriteInPlace,
                                 false,
                                 progress: OnFtpProgress);

                        if (status != FtpStatus.Success)
                        {
                            await notificationService.NotifyAsync(new Notification(correlationId, "Failed To Upload File".Error(), "FTPUpload".Success(), DateTime.Now));
                        }
                        else
                        {
                            await notificationService.NotifyAsync(new Notification(correlationId, $"File {relative.Success()} Uploaded", "FTPUpload".Success(), DateTime.Now));
                        }


                    }
                    catch (Exception ex)
                    {
                        var inner = ex.InnerException;
                        var sb = new StringBuilder();
                        sb.AppendLine(ex.Message).AppendLine(inner?.Message);
                        await notificationService.NotifyAsync($"{sb.ToString().Error()} for file {relative}");
                    }
                }
            }
        }
        client.Dispose();


    }
    void OnFtpProgress(FtpProgress ftpProgress)
    {

    }
    private static async Task<byte[]> ReadFileWithRetryAsync(string filePath, int maxRetries = 5, int delayMilliseconds = 200, CancellationToken cancellationToken = default)
    {
        int attempt = 0;
        while (true)
        {
            try
            {
                // Use FileStream with async read for true async IO
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
                var buffer = new byte[stream.Length];
                int read = 0;
                while (read < buffer.Length)
                {
                    int bytesRead = await stream.ReadAsync(buffer.AsMemory(read, buffer.Length - read), cancellationToken).ConfigureAwait(false);
                    if (bytesRead == 0)
                        break;
                    read += bytesRead;
                }
                return buffer;
            }
            catch (IOException ex) when (IsFileLocked(ex))
            {
                attempt++;
                if (attempt > maxRetries)
                    throw;
                await Task.Delay(delayMilliseconds, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static bool IsFileLocked(IOException ex)
    {
        int errorCode = System.Runtime.InteropServices.Marshal.GetHRForException(ex) & 0xFFFF;
        // ERROR_SHARING_VIOLATION = 32, ERROR_LOCK_VIOLATION = 33
        return errorCode == 32 || errorCode == 33;
    }
}
