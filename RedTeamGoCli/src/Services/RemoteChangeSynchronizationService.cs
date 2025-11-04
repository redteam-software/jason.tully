using System.Threading.Channels;

namespace RedTeamGoCli.Services;

[RegisterSingleton<IRemoteChangeSynchronizationService>]
public class RemoteChangeSynchronizationService : IRemoteChangeSynchronizationService
{
    private const string _defaultHost = "10.0.0.4";
    private const string _remoteDirectory = "mdrive\\paskrcustomers\\uatcode";
    private readonly FileChangeChannel _fileChangeChannel;
    private readonly IServiceProvider _serviceProvider;


    public RemoteChangeSynchronizationService(FileChangeChannel fileChangeChannel, IServiceProvider serviceProvider)
    {
        _fileChangeChannel = fileChangeChannel;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(SynchronizationParameters parameters, INotificationService notificationService, CancellationToken cancellationToken)
    {

        var target = _serviceProvider.GetRequiredKeyedService<IRemoteService>(parameters.RemoteTargetProject.Name);

        if (!await target.Initialize(parameters.RemoteTargetProject, notificationService))
        {
            await notificationService.NotifyAsync("Failed to initialize remote target".Error());
            return;
        }


        await notificationService.NotifyAsync(new Notification(Guid.NewGuid(), $"Batch Size:{parameters.BatchSize.NumericValue()}.  Debounce (secs): {parameters.DebounceSeconds.NumericValue()}"));

        await foreach (var batch in _fileChangeChannel.Reader.ReadAllBatches(parameters.BatchSize, TimeSpan.FromSeconds(parameters.DebounceSeconds), cancellationToken))
        {
            foreach (var changeSet in batch.GroupBy(x => x.FilePath))
            {
                var latest = changeSet.OrderByDescending(x => x.EventTime).First();
                var correlationId = latest.FilePath.ToLower();
                var relative = latest.FilePath.Replace(Environment.CurrentDirectory, string.Empty);


                var remotePath = BuildRemotePath(parameters.RemoteTargetProject.RemoteDirectory, relative);
                if (latest.ChageType == ChageType.Delete)
                {
                    await target.DeleteRemoteFile(latest.FilePath, parameters.RemoteTargetProject, notificationService);
                }
                else if (File.Exists(latest.FilePath))
                {

                    await target.UploadRemoteFile(new(latest.FilePath, relative, await ReadFileWithRetryAsync(latest.FilePath, 3, 200, cancellationToken)),
                         remotePath, parameters.RemoteTargetProject, notificationService);

                }
            }
        }

    }

    private static string BuildRemotePath(string remoteDirectory, string relativePath)
    {
        remoteDirectory = remoteDirectory.Trim();
        remoteDirectory = remoteDirectory.EndsWith("/") ? remoteDirectory.Substring(0, remoteDirectory.Length - 1) : remoteDirectory;

        relativePath = relativePath.Trim().Replace("\\", "/");
        relativePath = relativePath.StartsWith("/") ? relativePath.Substring(1) : relativePath;
        return $"{remoteDirectory}/{relativePath}".Replace("\\", "/");
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