using Cocona;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RedTeam.Extensions.Console.ExtensionMethods;
using RedTeamSecurityAnalyzer.Extensions;
using RedTeamSecurityAnalyzer.Forms;
using RedTeamSecurityAnalyzer.Reporting;
using RedTeamSecurityAnalyzer.Services.NotificationServices;
using RedTeamSecurityAnalyzer.TestExecution;
using Spectre.Console;
using System.Collections.Concurrent;
using System.Text;
using static RedTeamSecurityAnalyzer.Services.NotificationServiceExtensions;

namespace SecurityAuditCommand.Commands;

public record SecurityAuditCommandParameters(
        [Argument(Description = "The application name.  [go,flex,lens]")] string application = "go",
        [Option(Description = "username")] string? username = null,
        [Option(Description = "password")] string? password = null,
        string? logLevel = "information") : CommandParameters(logLevel);

[CommandHandler(CommandName, CommandDescription)]
public class SecurityAuditCommandCommand : ICommand<SecurityAuditCommandParameters>
{
    public const string CommandName = "analyze";
    public const string CommandDescription = "Audits a RedTeam app for path traversal vulnerabilities";

    private readonly IPuppeteerService _puppeteerService;
    private readonly ILoginFormFactory _loginFormFactory;
    private readonly IConfiguration _configuration;
    private readonly ITestRunnerFactory _testRunnerFactory;
    private readonly IRedTeamSecurityAnalysisTestCaseProvider _securityAnalaysisTestCaseProvider;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<SecurityAuditCommandCommand> _logger;
    private readonly DefaultCommandReport _securityAuditReportGenerator;

    public SecurityAuditCommandCommand(
        IPuppeteerService puppeteerService,
        ILoginFormFactory loginFormFactory,
        IConfiguration configuration,
        ITestRunnerFactory testRunnerFactory,
        IRedTeamSecurityAnalysisTestCaseProvider securityAnalaysisTestCaseProvider,
        ILoggerFactory loggerFactory,
        ILogger<SecurityAuditCommandCommand> logger,
        DefaultCommandReport securityAuditReportGenerator)
    {
        _puppeteerService = puppeteerService;
        _loginFormFactory = loginFormFactory;
        _configuration = configuration;
        _testRunnerFactory = testRunnerFactory;
        _securityAnalaysisTestCaseProvider = securityAnalaysisTestCaseProvider;
        _loggerFactory = loggerFactory;
        _logger = logger;
        _securityAuditReportGenerator = securityAuditReportGenerator;
    }

    public async Task RunAsync(SecurityAuditCommandParameters args, CommandContext commandContext, CancellationToken cancellationToken = default)
    {
        $"RedTeam.{args.application}".WriteApplicationLogo(new RedTeam.Extensions.Console.Fonts.TubesSmushed());
        $"{CommandName.Information()} - {CommandDescription} ({args.application})".WriteSubTitle(false);
        _logger.LogInformation("Starting Security Audit Command for {application}", args.application);
        var upn = Upn.Create(args.username, args.password, _configuration);
        var testCases = _securityAnalaysisTestCaseProvider.RedTeamSecurityAnalysisTestCases(args.application);

        if (!testCases.Any())
        {
            $"No Test Cases Are Enabled".WriteLine();
            return;
        }

        var browser = await _puppeteerService.GetBrowser(false, EmptyNotificationService.Default);

        var taskDescriptionColumn = new TaskDescriptionColumn();
        taskDescriptionColumn.Alignment = Justify.Left;

        var aggregatedTestResults = new ConcurrentBag<TestRunnerResponse>();
        await AnsiConsole.Progress().AutoRefresh(true)
            .Columns(new ProgressColumn[]
            {
                taskDescriptionColumn,
                new SpectreConsoleProgressTaskMessageColumn(),
                 new PercentageColumn(),
                //new SecurityAnalysisStatusColumn()
            })
            .StartAsync(async ctx =>
            {
                var flattenedTestCases = from testCase in testCases
                                         from rule in testCase.Rules
                                         select (r: rule, t: testCase);

                var results = await Task.WhenAll(flattenedTestCases.Select(DispatchTestRunner));

                foreach (var result in results.SelectMany(x => x))
                {
                    aggregatedTestResults.Add(result);
                }

                async Task<List<TestRunnerResponse>> DispatchTestRunner((RedTeamSecurityAnalysisRule r, RedTeamSecurityAnalysisTestCase t) testCase)
                {
                    var testResults = new ConcurrentBag<TestRunnerResponse>();
                    var testRunner = _testRunnerFactory.GetTestRunner(testCase.t.TestRunner);
                    var task = ctx.AddTask($"{testCase.t.Name} [yellow]{testCase.r.Name}[/]").InitializeTask("Initialized Test Runner" + testCase.t.TestRunner);
                    task.MaxValue = 1;
                    var progressTaskNotification = new ProgressTaskNotificationService(task, _loggerFactory);

                    var results = await testRunner.ExecuteAsync(testCase.t, new TestRunnerContext(RedTeamApplication.Go, progressTaskNotification, _loginFormFactory, browser, upn, null));
                    task.Increment(1);
                    task.StopTask();

                    if (results?.Any() == true)
                    {
                        foreach (var result in results)
                        {
                            testResults.Add(result);
                        }
                    }
                    return testResults.ToList();
                }
            });


        await GenerateReport(aggregatedTestResults, RedTeamApplicationUtil.Parse(args.application));
    }

    private async Task GenerateReport(IEnumerable<TestRunnerResponse> results, RedTeamApplication redTeamApplication)
    {
        var testSummary = results.CalculateTotals();
        var (totalRules, totalTestCases, metrics) = testSummary;


        _logger.LogInformation("Completed Security Audit Command.");

        var summaryBuilder = new StringBuilder();

        summaryBuilder
            .AppendLine("[yellow]Security Audit Summary[/]")
            .AppendLine($"Total Rules Executed: [bold]{totalRules}[/]")
            .AppendLine($"Total Test Cases Executed: [bold]{totalTestCases}[/]");

        foreach (var (total, rate, label, status) in metrics.OrderBy(x => x.Status))
        {
            var color = status switch
            {
                SecurityAnalysisStatus.Passed => "green",
                SecurityAnalysisStatus.Failed => "red",
                _ => "yellow"
            };
            summaryBuilder.AppendLine($"{label} [{color}]{total} ({rate:N2}%)[/]");
        }

        var summary = new Panel(new Markup(summaryBuilder.ToString()))
        {
            Header = new PanelHeader("Summary", Justify.Center),
            Padding = new Padding(1, 1),
            Border = BoxBorder.Rounded,
            Expand = true
        };
        AnsiConsole.Write(summary);

        //now lets generate an html report from results

        var report = await _securityAuditReportGenerator.GetGeneratedReportAsync(results, testSummary, $"{redTeamApplication.ToString()} Security Analysis Report - {DateTime.Now}", "../../");

        report.FilePath.OpenInBrowser(); ;
    }
}