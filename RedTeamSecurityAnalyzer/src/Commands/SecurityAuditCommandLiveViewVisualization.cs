using Microsoft.Extensions.Configuration;
using RedTeamGoCrawler.Components;
using RedTeamGoCrawler.SecurityRules;
using RedTeamGoCrawler.Services;
using SecurityAuditCommand.Commands;
using Spectre.Console;
using System.Collections.Concurrent;

namespace RedTeamGoCrawler.Commands;

[RegisterSingleton]
public class SecurityAuditCommandLiveViewVisualization
{
    private readonly IGoBrowserService _goBrowserService;
    private readonly IConfiguration _configuration;
    private readonly ISecurityRuleFactory _securityRuleFactory;

    public SecurityAuditCommandLiveViewVisualization(IGoBrowserService goBrowserService, IConfiguration configuration, ISecurityRuleFactory securityRuleFactory)
    {
        _goBrowserService = goBrowserService;
        _configuration = configuration;
        _securityRuleFactory = securityRuleFactory;
    }

    public async Task RunAsync(
        SecurityAuditCommandParameters args,
        string commandName,
        string commandDescription, CancellationToken cancellationToken = default)
    {
        var upn = Upn.Create(args.username, args.password, _configuration);

        var browser = await _goBrowserService.GetBrowser();



        //  var table = SecurityRuleFactory.DescribeRules().DictionaryToTable("Code", "Name", "Path Traversal Detection Rules");

        var table = new PathTraversalRuleTable(_securityRuleFactory.DescribeRules());

        var splitLayout = new SplitLayout();
        splitLayout.UpdateRightPanel(table);

        var resultBag = new ConcurrentBag<SecurityAnalysisResult>();

        await AnsiConsole.Live(splitLayout).StartAsync(async ctx =>
        {

            ctx.Refresh();
            var leftPanelLogger = new SpectorTableLogger(ctx);

            var upn = Upn.Create(args.username, args.password, _configuration);

            splitLayout.UpdateLeftPanel(leftPanelLogger.Render());

            foreach (var rule in _securityRuleFactory.GetPathTraversalRules())
            {
                //invoke them
                var response = await rule.rule.RunAsync(null, browser, leftPanelLogger);

                table.UpdateRow(response.Rule, response.Status);
                splitLayout.UpdateRightPanel(table);
                ctx.Refresh();

                resultBag.Add(response);
            }


            splitLayout.Clear();
            ctx.Refresh();
        });

        //process complete. let's make a pretty summary.
        // CreateResultsSummary(resultBag, SecurityAnalysisStatus.Passed, "Passing Rules", r => r.Passing, "green");
        // CreateResultsSummary(resultBag, SecurityAnalysisStatus.Failed, "Failing Rules", r => r.Failing, "red");

        //var tablePassed = new Table().HideHeaders().AddColumn("").Expand().Border(TableBorder.None);
        //var tableFailed = new Table().HideHeaders().AddColumn("").Expand().Border(TableBorder.None);

        //var tablePassedRule = new Rule("Passing Rules").RuleStyle("green");
        //var tableFailedRule = new Rule("Failing Rules").RuleStyle("red");


        Console.Clear();

        var t = new Table().AddColumns("Rule", "Pass%", "Failed%").Expand().Border(TableBorder.None);

        foreach (var rule in resultBag.OrderBy(x => x.Rule))
        {
            var passed = rule.ChildResults.Where(x => x.Status == SecurityAnalysisStatus.Passed);

            var failed = rule.ChildResults.Where(x => x.Status == SecurityAnalysisStatus.Failed);

            var passRate = (double)passed.Count() / rule.ChildResults.Count() * 100;
            var failRate = (double)failed.Count() / rule.ChildResults.Count() * 100;

            t.AddRow(
                $"{rule.Rule.ToString().TextValue()} ({rule.ChildResults.Count.NumericValue()})",
                $"{passed.Count().ToString("N0").Success()} Passed ({passRate.NumericValue()}%)",
                $"{failed.Count().ToString("N0").Error()} Failed ({failRate.NumericValue()}%");

        }


        AnsiConsole.Write(t);

        //foreach (var item in resultBag.OrderBy(x => x.Rule))
        //{
        //    var passed = item.ChildResults.Where(x => x.Status == SecurityAnalysisStatus.Passed);

        //    var failed = item.ChildResults.Where(x => x.Status == SecurityAnalysisStatus.Failed);

        //    tablePassed.AddRow(new Markup($"[green]{item.Rule}[/]"));
        //    foreach (var p in passed)
        //    {
        //        tablePassed.AddRow(new Markup($"  [green]*[/] {p.Message}"));
        //    }
        //    if (failed.Any())
        //    {
        //        tableFailed.AddRow(new Markup($"[red]{item.Rule}[/]"));
        //        foreach (var f in failed)
        //        {

        //            tableFailed.AddRow(new Markup($"  [red]X[/] {f.Message}"));
        //        }
        //    }
        //}

        //var totalRulesExecuted = resultBag.SelectMany(x => x.ChildResults).Count();
        //var totalPassed = resultBag.SelectMany(x => x.ChildResults).Count(x => x.Status == SecurityAnalysisStatus.Passed);
        //var totalFailed = resultBag.SelectMany(x => x.ChildResults).Count(x => x.Status == SecurityAnalysisStatus.Failed);
        //var failureRate = (double)totalFailed / totalRulesExecuted * 100;
        //var successRate = (double)totalPassed / totalRulesExecuted * 100;
        //var summary = new Markup($"[bold]Total Rules Executed:[/] {totalRulesExecuted}\n[green]Total Passed:[/] {totalPassed}\n[red]Total Failed:[/] {totalFailed}\n[red]Failure Rate:[/] {failureRate:0.00}%\n[green]Success Rate:[/] {successRate:0.00}%\n");
        //AnsiConsole.Write(summary);


        //var tableRows = new Panel(tablePassed).Expand();
        //tableRows.Header = new PanelHeader("Passing Rules", Justify.Center);
        //var tableFailedRows = new Panel(tableFailed).Expand();
        //tableFailedRows.Header = new PanelHeader("Failing Rules", Justify.Center);

        //var layout = new Layout("Root")
        //   .SplitColumns(
        //       new Layout("Passed Rules", tableRows),
        //       new Layout("Failed Rules", tableFailedRows)
        //   );

        // AnsiConsole.Write(layout);
    }

    private static void CreateResultsSummary(IEnumerable<SecurityAnalysisResult> results,
        SecurityAnalysisStatus statusFilter,
        string title,
        Func<SecurityAnalysisResult, int> valueAccessor)
    {
        var rule = new Rule(title).RuleStyle("red");
        AnsiConsole.Write(rule);
        var chart = new BreakdownChart().Expand();



        foreach (var r in results.Where(x => valueAccessor(x) > 0))
        {



            var color = r.Rule switch
            {
                PathTraversalRule.Basic => Color.Green,
                PathTraversalRule.URLEncoded => Color.Green3,
                PathTraversalRule.Unicode => Color.Green4,
                PathTraversalRule.HTMLEntities => Color.Yellow,
                PathTraversalRule.NullBytes => Color.Olive,
                PathTraversalRule.Advanced => Color.Orange1,
                PathTraversalRule.ColdFusion => Color.Orange4_1,
                PathTraversalRule.FileUpload => Color.Blue1,
                PathTraversalRule.MultiStage => Color.Purple,
                PathTraversalRule.RateLimit => Color.Magenta1,
                PathTraversalRule.FormData => Color.Teal,
                PathTraversalRule.MultipartForms => Color.CadetBlue,
                PathTraversalRule.JSONPayload => Color.DarkMagenta,
                PathTraversalRule.CFFormFields => Color.GreenYellow,
                PathTraversalRule.PostSizeLimit => Color.Gold1,
                _ => Color.Red
            };



            chart = chart.AddItem(r.Rule.ToString(), valueAccessor(r), color);
        }

        AnsiConsole.Write(chart);
    }
}
