using RedTeamSecurityAnalyzer.Extensions;
using RedTeamSecurityAnalyzer.TestExecution;
using System.Text;
using System.Text.RegularExpressions;

namespace RedTeamSecurityAnalyzer.Reporting;

[RegisterSingleton]
public class DefaultCommandReport : ICommandReport<TestSummary>
{

    private const string _reportTemplate = """
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="UTF-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1.0" />
          <title>{reportTitle}</title>
          <script src="https://cdn.jsdelivr.net/npm/alpinejs@3.x.x/dist/cdn.min.js" defer></script>
          <script src="https://cdn.jsdelivr.net/npm/@tailwindcss/browser@4"></script>
          <style>
                .passed { color: green; }
                .failed { color: red; }
                .internalservererror { color: blue; }
                 .notrun { color: orange; }
                 .info-log{ color: blue; }
                 .default-log{ color: green; }
                 .error-log{ color: red; font-weight: bold; }
                 .warning-log{ color: orange; font-weight: bold; }
                 .time-stamp{ color: magenta; }
                 .log-message{ color: black; }

          </style>
        </head>
        <body class="bg-gray-100 p-6">
            <h1 class='font-black text-4xl mb-4'>{reportTitle}</h1>
          <div x-data="{ tab: 'report' }" class="w-full bg-white shadow rounded-lg">
            <!-- Tabs -->
            <div class="border-b flex">
              <button @click="tab = 'report'" :class="tab === 'report' ? 'border-blue-500 text-blue-500' : 'text-gray-600'" class="py-2 px-4 border-b-2 font-medium focus:outline-none">
                Report
              </button>
              <button @click="tab = 'logs'" :class="tab === 'logs' ? 'border-blue-500 text-blue-500' : 'text-gray-600'" class="py-2 px-4 border-b-2 font-medium focus:outline-none">
                Logs
              </button>
            
            </div>
        
            <!-- Tab Panels -->
            <div class="p-4">
              <div x-show="tab === 'report'" >
                {report}
              </div>
              <div x-show="tab === 'logs'">
                <h2 class="text-xl font-semibold mb-2">Output Logs</h2>
            {log}
              </div>
             
            </div>
          </div>
        
        </body>
        </html>
        """;
    public async Task<GeneratedReport> GetGeneratedReportAsync(IEnumerable<TestRunnerResponse> Results, TestSummary reportData, string reportTitle, string reportDirectory, CancellationToken cancellationToken = default)
    {
        var (totalRulesExecuted, totalTestCases, metrics) = reportData;

        var html = new StringBuilder(_reportTemplate).Replace("{reportTitle}", reportTitle);

        var reportBuilder = new StringBuilder();

        var summaryBuilder = new StringBuilder("<div>");

        summaryBuilder
            .AppendLine($"Total Rules Executed: {totalRulesExecuted}")
            .AppendLine($"Total Test Cases Executed: {totalTestCases}");

        foreach (var (total, rate, label, status) in metrics.OrderBy(x => x.Status))
        {
            var color = status switch
            {
                SecurityAnalysisStatus.Passed => "green",
                SecurityAnalysisStatus.Failed => "red",
                _ => "orange"
            };
            summaryBuilder.AppendLine($"<div> {label} <span class=\"font-bold text-{color}-900\">{total}</span><span class=\"font-bold text-{color}-900\"> ({rate:N2}%)<span></div>");
        }

        summaryBuilder
            .AppendLine("<hr class=\"mt-2 mb-2 border-t-2 border-gray-600\" style=\"width: 650px;\"  />")
            .AppendLine("</div>");

        var sanitizedTitle = SanitizeReportTitle(reportTitle);
        var reportPath = Path.Combine(reportDirectory, $"{sanitizedTitle}.html");




        reportBuilder.AppendLine(summaryBuilder.ToString());

        var groupedTestCases = Results.GroupBy(x => x.TestCase.Name).ToList();

        foreach (var testCaseItem in groupedTestCases)
        {
            var content = $"""
                <div>
                  <div class='font-black text-2xl'>Test Case: {testCaseItem.Key}</div>
                  #rules#

                </div>
                """;

            var ruleBuilder = new StringBuilder();
            foreach (var rule in testCaseItem.OrderBy(x => x.Rule.Id))
            {
                var ruleHeader = $"<div class='ml-2 font-black text-xl'>{rule.Rule.Name} - {rule.Rule.Description}</div>";
                ruleBuilder.AppendLine(ruleHeader);

                var joinResponsesToRuleTestData = from response in rule.TestCaseResponses
                                                  where response.Rule.Name == rule.Rule.Name
                                                  join testData in rule.Rule.TestData on new
                                                  {
                                                      response.TestData.Pattern,
                                                  } equals new
                                                  {
                                                      testData.Pattern,
                                                  }

                                                  select new
                                                  {
                                                      response,
                                                      testData
                                                  };

                var table = new StringBuilder("<div class=\"ml-5 grid grid-cols-2\">");

                foreach (var data in joinResponsesToRuleTestData.ToList())
                {
                    var statusClass = data.response.Status == SecurityAnalysisStatus.Passed ? "passed" : "failed";
                    table.AppendLine($"<div>{data.response.TestData.Pattern}</div> <div> <span class='{statusClass}'>{data.response.Status.ToString()}</span></div>");
                }
                ruleBuilder.AppendLine(table.ToString()).AppendLine("</div>").AppendLine("<hr class=\"mt-2 mb-2 border-t-2 border-gray-600\" />");

                //var statusClass = rule.Rule..Status == SecurityAnalysisStatus.Passed ? "passed" : "failed";
                //childHtml.AppendLine($"<li>{StripHtml(child.Message)} </li>");
            }
            var testCase = content.Replace("#rules#", ruleBuilder.ToString());
            reportBuilder.AppendLine(testCase);

            //var panel = $"""
            //      <div class="mt-2">
            //      <h2 class="text-xl font-semibold">Rule: {result.Rule.ToString()} - {descriptions.FirstOrDefault(x => x.Rule == result.Rule).Description}</h2>
            //      <h4 class='{result.Status.ToString().ToLower()}'> {StripHtml(result.Message)}</h4>
            //      {childHtml.ToString()}
            //      <hr class="mt-2 mb-2 border-t-2 border-gray-600" style="width: 650px;"  />
            //    </div>

            //    """;
        }

        var logFile = ExtractCurrentLogFile();


        var text = reportBuilder.ToString();
        var htmlText = html.ToString();

        html.Replace("{log}", $"{logFile}")
              .Replace("{report}", reportBuilder.ToString());


        var output = html.ToString();


        await File.WriteAllTextAsync(reportPath, html.ToString());
        return new GeneratedReport(new FileInfo(reportPath).FullName, reportTitle);
    }
    public static string SanitizeReportTitle(string reportTitle)
    {
        if (string.IsNullOrWhiteSpace(reportTitle))
            return "Untitled";

        // Remove invalid filename characters: \ / : * ? " < > | and control chars
        string sanitized = Regex.Replace(reportTitle, @"[\\/:*?""<>|\r\n]+", "");

        // Optionally replace spaces with underscores
        sanitized = Regex.Replace(sanitized, @"\s+", "_");

        // Trim leading/trailing underscores
        return sanitized.Trim('_');
    }


    private static string? ExtractCurrentLogFile()
    {

        var logBuilder = new StringBuilder();
        var logDirectory = new DirectoryInfo("..\\..\\..\\");

        var files = logDirectory.GetFiles("*.html");

        var logFile = files.OrderByDescending(x => x.LastWriteTime).FirstOrDefault();

        if (logFile != null)
        {
            var temp = Path.GetTempFileName();
            try
            {
                logFile.CopyTo(temp, true);
                using var fs = new FileStream(temp, FileMode.Open, FileAccess.Read);
                using var sr = new StreamReader(fs);
                logBuilder.AppendLine(sr.ReadToEnd());
            }
            finally
            {
                try
                {
                    File.Delete(temp);
                }
                catch
                {

                }
            }
        }

        var logHtml = logBuilder.ToString();

        logHtml = ConvertUrlsToLinks(logHtml);


        return logHtml;

        string ConvertUrlsToLinks(string input)
        {
            string pattern = @"(https?://[^\s]+)";
            string replacement = "<a class='text-blue-500'  href=\"$1\" target=\"_blank\">$1</a>";

            return Regex.Replace(input, pattern, replacement);
        }
    }

    private static string StripHtml(string input)
    {
        var html = input.Trim();
        if (html.Contains("<html"))
        {
            //var index = html.IndexOf("</html>");
            //if (index > 0)
            //{
            //    return html.Substring(0, index + 7);
            //}
            return "Rule caused an internal server error and could not be completed.  This occurs when the WAF rules have been disabled.";
        }
        return input;
    }
}



