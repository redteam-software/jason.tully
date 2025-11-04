using System.Collections.Concurrent;

namespace RedTeamGoCli.Services;

public class LiveTableNotificationService : INotificationService
{
    private readonly LiveDisplayContext _liveDisplayContext;
    private readonly Table _table;
    private readonly int _maxiumRows;
    private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

    private ConcurrentDictionary<NotificationId, int> _rowMap = new ConcurrentDictionary<NotificationId, int>();

    public LiveTableNotificationService(LiveDisplayContext liveDisplayContext, Table table, int maxiumRows)
    {
        _liveDisplayContext = liveDisplayContext;
        _table = table;
        _maxiumRows = maxiumRows;
    }

    public async Task NotifyAsync(Notification notification, CancellationToken? cancellationToken = null)
    {
        await semaphore.WaitAsync(); // Wait for access

        _rowMap.AddOrUpdate(notification.Id, 1, (key, oldValue) => oldValue + 1);

        try
        {
            if (_table.Rows.Count >= _maxiumRows)
            {
                _table.Rows.RemoveAt(0);
                var entry = _rowMap.FirstOrDefault(x => x.Value == 0);
                _rowMap.TryRemove(entry);
            }

            var eventTime = notification.EventTime ?? DateTime.Now;


            _table.AddRow(new Markup(notification.Message), new Markup(eventTime.ToString()));


            this._liveDisplayContext.Refresh();
        }
        finally
        {
            semaphore.Release();
        }
    }
}