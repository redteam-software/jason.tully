using RedTeamSecurityAnalyzer.Extensions;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RedTeamSecurityAnalyzer.Components;

internal class PageInformation : IRenderable
{
    private readonly IPage _page;

    private IRenderable? _renderable;

    public PageInformation(IPage page)
    {
        _page = page;
    }

    public async Task LoadAsync()
    {
        var cookies = await _page.GetCookiesAsync();
        var headers = await _page.GetResponseHeaders();

        var rows = new Rows(cookies.CookieParamsToTable("Cookies"), headers.HeadersToTable("Response Headers")).Expand();

        var panel = new Panel(rows);
        panel.Header = new PanelHeader($"Path: ${new Uri(_page.Url).PathAndQuery.Success()}").LeftJustified();

        _renderable = panel;
    }

    public Measurement Measure(RenderOptions options, int maxWidth)
    {
        return new Measurement(maxWidth, maxWidth);
    }

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        if (_renderable == null)
        {
            return Enumerable.Empty<Segment>();
        }
        return _renderable.Render(options, maxWidth);
    }
}