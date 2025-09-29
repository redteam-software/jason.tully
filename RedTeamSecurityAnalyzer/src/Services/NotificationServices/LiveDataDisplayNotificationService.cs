using Spectre.Console;
using Spectre.Console.Rendering;

namespace RedTeamSecurityAnalyzer.Services.NotificationServices;

internal class LiveDataDisplayNotificationService : ILiveDisplayNotificationService
{
    public LiveDisplayContext LiveDisplayContext { get; init; }
    public IRenderable Renderable => _table;

    private readonly Table _table;

    public LiveDataDisplayNotificationService(LiveDisplayContext liveDisplayContext, Table renderable)
    {
        LiveDisplayContext = liveDisplayContext;
        _table = renderable;
    }

    public Task NotifyAsync(IRenderable message)
    {
        if (_table.Rows.Count > 50)
        {
            _table.Rows.Clear();
            //this.Renderable.Rows.RemoveAt(0);
        }
        this._table.AddRow(message);

        this.LiveDisplayContext.Refresh();
        return Task.CompletedTask;
    }

    public Task NotifyAsync(Dictionary<string, object> state)
    {
        if (state.ContainsKey("Message"))
        {
            if (_table.Rows.Count > 50)
            {
                _table.Rows.Clear();
                //this.Renderable.Rows.RemoveAt(0);
            }
            this._table.AddRow(new Markup(state["Message"].ToString()!));

            this.LiveDisplayContext.Refresh();
        }
        return Task.CompletedTask;
    }

    public void Exception(Exception ex)
    {

    }

    public void Information(string message)
    {

    }
}