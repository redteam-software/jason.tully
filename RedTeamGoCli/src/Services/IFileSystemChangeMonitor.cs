using System.Collections.Concurrent;
using System.Threading.Channels;

namespace RedTeamGoCli.Services;

public enum ChageType
{
    Create,
    Update,
    Delete,
    Rename
}
public record FileChange(string FilePath, ChageType ChageType, DateTime EventTime, string? OldPath = null);



[RegisterSingleton]
public class FileChangeChannel
{
    public ChannelReader<FileChange> Reader { get; init; }
    public ChannelWriter<FileChange> Writer { get; init; }

    public FileChangeChannel()
    {
        var channel = Channel.CreateUnbounded<FileChange>();

        Reader = channel.Reader;
        Writer = channel.Writer;
    }
}

public interface IFileSystemChangeMonitor
{
    Task MonitorAsync(string directoryPath, INotificationService notificationService, CancellationToken cancellationToken);
}

[RegisterSingleton<IFileSystemChangeMonitor>]
public class FileSystemChangeMonitor : IFileSystemChangeMonitor
{
    private static readonly TimeSpan DebounceWindow = TimeSpan.FromMilliseconds(500);
    private static readonly ConcurrentDictionary<string, CancellationTokenSource> _debounceTokens = new();
    public FileChangeChannel Channel { get; }

    public FileSystemChangeMonitor(FileChangeChannel channel)
    {
        Channel = channel;
    }

    public async Task MonitorAsync(string directoryPath, INotificationService notificationService, CancellationToken cancellationToken)
    {

        var watcher = new FileSystemWatcher
        {
            Path = directoryPath,            // Directory to monitor
            //Filter = "*.txt",               // File type filter
            IncludeSubdirectories = true,   // Watch subfolders
            EnableRaisingEvents = true      // Start monitoring
        };

        try
        {
            watcher.Created += OnCreated;
            watcher.Changed += OnChanged;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;

            await notificationService.NotifyAsync($"Monitoring {directoryPath.TextValue(true)} for Changes");

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000);
            }


        }
        catch (Exception ex)
        {
            await notificationService.NotifyAsync($"File System Watcher Error: {ex.Message}".Error());
        }
        finally
        {
            watcher.Created -= OnCreated;
            watcher.Changed -= OnChanged;
            watcher.Deleted -= OnDeleted;
            watcher.Renamed -= OnRenamed;
            Channel.Writer.TryComplete();
            watcher.Dispose();
        }




    }
    void OnCreated(object sender, FileSystemEventArgs e) =>
      FilterEvent(e, (e) => Channel.Writer.TryWrite(new FileChange(e.FullPath, ChageType.Create, DateTime.UtcNow)));

    void OnChanged(object sender, FileSystemEventArgs e) =>
        FilterEvent(e, (e) => Channel.Writer.TryWrite(new FileChange(e.FullPath, ChageType.Update, DateTime.UtcNow)));


    void OnDeleted(object sender, FileSystemEventArgs e) =>
      FilterEvent(e, (e) => Channel.Writer.TryWrite(new FileChange(e.FullPath, ChageType.Delete, DateTime.UtcNow)));

    void OnRenamed(object sender, RenamedEventArgs e) =>
           FilterEvent(e, (e) => Channel.Writer.TryWrite(new FileChange(e.FullPath, ChageType.Rename, DateTime.UtcNow)));


    private void FilterEvent(FileSystemEventArgs e, Action<FileSystemEventArgs> publish)
    {
        if (e.Name != null && e.Name.StartsWith(".git"))
        {
            // Ignore .git directory events
            return;
        }
        if (Directory.Exists(e.FullPath))
        {
            // Ignore directory events
            return;
        }

        if (e.ChangeType == WatcherChangeTypes.Changed)
        {
            Debounce(e, publish);
            return;
        }

        publish(e);
    }


    private void Debounce(FileSystemEventArgs e, Action<FileSystemEventArgs> publish)
    {
        // Cancel any pending event for this file and start a new debounce window
        var cts = _debounceTokens.AddOrUpdate(
            e.FullPath,
            _ => new CancellationTokenSource(),
            (_, existingCts) =>
            {
                existingCts.Cancel();
                existingCts.Dispose();
                return new CancellationTokenSource();
            });

        var token = cts.Token;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(DebounceWindow, token);
                // Only publish if not cancelled in the debounce window
                publish(e);
            }
            catch (TaskCanceledException)
            {
                // Swallow, as this means a new event arrived within the debounce window
            }
            finally
            {
                _debounceTokens.TryRemove(e.FullPath, out _);
            }
        });
        return;
    }

}
