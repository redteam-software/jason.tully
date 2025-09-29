using Spectre.Console;
using Spectre.Console.Rendering;

namespace RedTeamSecurityAnalyzer.Components;

public class PathTraversalRuleTable : IRenderable
{
    private readonly IDictionary<PathTraversalRule, string> _rules;
    private Table _table;
    private Dictionary<PathTraversalRule, int> _tableRowMap = new Dictionary<PathTraversalRule, int>();

    private const int CellCode = 0;
    private const int CellName = 1;
    private const int CellStatus = 2;

    public PathTraversalRuleTable(IDictionary<PathTraversalRule, string> rules)
    {
        _rules = rules;
        _table = new Table()
           .HideHeaders()
           .AddColumns("Code", "Name", "Status")
           .Expand()
           .Border(TableBorder.None);

        int counter = 0;
        foreach (var kvp in _rules)
        {
            var statusString = FormatSecurityAnalysisStatusResult(SecurityAnalysisStatus.Initialized);
            _table.AddRow(((int)kvp.Key).ToString().NumericValue(), kvp.Value.TextValue(), statusString);

            _tableRowMap[kvp.Key] = counter;
            counter++;
        }
    }

    public void UpdateRow(PathTraversalRule rule, SecurityAnalysisStatus securityAnalysisStatus)
    {
        var index = _tableRowMap[rule];
        _table.Rows.Update(index, CellStatus, new Markup(FormatSecurityAnalysisStatusResult(securityAnalysisStatus)));
    }

    private static string FormatSecurityAnalysisStatusResult(SecurityAnalysisStatus securityAnalysisStatus)
    {
        var statusString = securityAnalysisStatus switch
        {
            SecurityAnalysisStatus.Passed => "Passed".Success(),
            SecurityAnalysisStatus.Failed => "Failed".Error(),
            SecurityAnalysisStatus.NotRun => "Not Run".Warning(),
            SecurityAnalysisStatus.Initialized => "Initialized".TextValue(),
            _ => "Unknown".TextValue()
        };

        return statusString;
    }

    public Measurement Measure(RenderOptions options, int maxWidth)
    {
        return new Measurement(maxWidth, maxWidth);
    }

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        return ((IRenderable)_table).Render(options, maxWidth);
    }
}