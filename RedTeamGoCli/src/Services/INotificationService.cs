using System.Collections.Concurrent;

namespace RedTeamGoCli.Services;

public record NotificationId(string Value)
{
    public static NotificationId System => new NotificationId("0");

    public static implicit operator NotificationId(string value)
    {
        return new NotificationId(value);
    }
    public static implicit operator NotificationId(Guid guid) => new NotificationId(guid.ToString());
}
public record Notification(NotificationId Id, string Message, string Category = "System", DateTime? EventTime = null, bool ReplaceMessage = true)
{

}
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


public class LiveTableNotificationService : INotificationService
{
    private readonly LiveDisplayContext _liveDisplayContext;
    private readonly Table _table;
    private readonly int _maxiumRows;
    static SemaphoreSlim semaphore = new SemaphoreSlim(1);

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

        try
        {


            if (_table.Rows.Count >= _maxiumRows)
            {
                _table.Rows.RemoveAt(0);
                var entry = _rowMap.FirstOrDefault(x => x.Value == 0);
                _rowMap.TryRemove(entry);
            }

            var eventTime = notification.EventTime ?? DateTime.Now;

            if (notification.ReplaceMessage)
            {
                //attempt to locate a corresponding notification id in our table.
                if (_rowMap.TryGetValue(notification.Id, out var row))
                {
                    _table.RemoveRow(row);
                    _table.InsertRow(row, new Markup(notification.Message), new Markup(eventTime.ToString()));
                }
                else
                {
                    _table.AddRow(new Markup(notification.Message), new Markup(eventTime.ToString()));
                    _rowMap.AddOrUpdate(notification.Id, _table.Rows.Count - 1, (k, v) => _table.Rows.Count - 1);
                }
            }
            else
            {

                _table.AddRow(new Markup(notification.Message), new Markup(eventTime.ToString()));

            }


            this._liveDisplayContext.Refresh();
        }
        finally
        {
            semaphore.Release();
        }

    }

}
