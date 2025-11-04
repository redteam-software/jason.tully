using System.Collections.Concurrent;

namespace RedTeamGoCli.Services;
internal class BarChartNotificationService : INotificationService
{
    private readonly LiveDisplayContext _liveDisplayContext;
    private readonly Table _table;
    private readonly Table _messageTable;

    private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

    private ConcurrentDictionary<NotificationId, int> _rowMap = new ConcurrentDictionary<NotificationId, int>();

    public BarChartNotificationService(LiveDisplayContext liveDisplayContext, Table table, int maxiumRows)
    {
        _liveDisplayContext = liveDisplayContext;
        _table = table;
        _messageTable = new Table().Expand().HideRowSeparators().NoBorder();
        _messageTable.AddColumns("Message", "Time");




        _table.AddEmptyRow();
        _table.AddEmptyRow();
    }

    public async Task NotifyAsync(Notification notification, CancellationToken? cancellationToken = null)
    {
        await semaphore.WaitAsync(); // Wait for access

        if (notification.Category == "FileUpload")
        {
            var value = _rowMap.AddOrUpdate(notification.Id, 1, (key, oldValue) => oldValue + 1);

            var chart = new BarChart()
            .Width(120)
            .Label("Uploads")
            .CenterLabel()
            .ShowValues().AddItems(_rowMap, (item) => new BarChartItem(item.Key.Value, item.Value, Color.Yellow));
            _table.UpdateCell(1, 0, chart);

        }
        else
        {
            _messageTable.AddRow(new Markup(notification.Message), new Markup((notification.EventTime ?? DateTime.Now).ToString()));
            _table.UpdateCell(0, 0, _messageTable);
        }


        try
        {

            _liveDisplayContext.Refresh();
        }
        finally
        {
            semaphore.Release();
        }
    }
}
