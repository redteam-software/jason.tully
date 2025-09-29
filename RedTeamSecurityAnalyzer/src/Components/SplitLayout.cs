using Spectre.Console;
using Spectre.Console.Rendering;
using System.Collections.Concurrent;

namespace RedTeamSecurityAnalyzer.Components;

public enum LayoutLocations
{
    Root,
    Left,
    Right,
    RightTop,
    RightBottom
}

public class SplitLayout : Renderable
{
    private static object syncRoot = new();
    private Layout _rootLayout;

    private readonly ConcurrentDictionary<string, UpdatablePane> _panes = new();

    public SplitLayout()
    {
        var layoutLeft = new Layout(LayoutLocations.Left.ToString(), new Rows().Expand());
        var layoutRight = new Layout(LayoutLocations.Right.ToString(), new Rows().Expand());

        _rootLayout = new Layout(LayoutLocations.Root.ToString())
              .SplitColumns(
                  layoutLeft,
                  layoutRight);
    }

    public void Clear()
    {
        lock (syncRoot)
        {
            _rootLayout = new Layout("empty", new Text(""));
        }
    }

    public void UpdateLeftPanel(IRenderable component) => UpdatePanel(component, LayoutLocations.Left);

    public void UpdateRightPanel(IRenderable component) => UpdatePanel(component, LayoutLocations.Right);

    private void UpdatePanel(IRenderable component, LayoutLocations name)
    {
        lock (syncRoot)
        {
            try
            {
                _rootLayout[name.ToString()].Update(component);
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
            }
        }
    }

    protected override Measurement Measure(RenderOptions options, int maxWidth)
    {
        return new Measurement(maxWidth, maxWidth);
    }

    /// <inheritdoc/>
    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var r = (IRenderable)_rootLayout;

        return r.Render(options, maxWidth);
    }
}