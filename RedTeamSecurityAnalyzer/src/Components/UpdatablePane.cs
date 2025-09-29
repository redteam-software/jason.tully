using Spectre.Console;
using Spectre.Console.Rendering;

namespace RedTeamSecurityAnalyzer.Components;

public class UpdatablePane : IRenderable
{
    private readonly Layout _parent;
    private Table _table = new Table().Expand().Border(TableBorder.None).HideHeaders().AddColumns("");

    public UpdatablePane(Layout parent)
    {
        _parent = parent;
    }

    public void Update(IRenderable component, int childLimit = 1)
    {
        if (childLimit > 0 && _table.Rows.Count >= childLimit)
        {
            _table.Rows.RemoveAt(0);
        }
        _table.AddRow(component);
    }

    public Measurement Measure(RenderOptions options, int maxWidth)
    {
        return new Measurement(maxWidth, maxWidth);
    }

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        return ((IRenderable)_table).Render(options, maxWidth);
    }

    public static implicit operator UpdatablePane(Layout layout) => new UpdatablePane(layout);
}