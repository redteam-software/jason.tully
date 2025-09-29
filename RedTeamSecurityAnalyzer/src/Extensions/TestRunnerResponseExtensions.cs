using RedTeamSecurityAnalyzer.TestExecution;

namespace RedTeamSecurityAnalyzer.Extensions;

public record TestSummary(int TotalRules, int TotalTestCases, List<ResponseMetrics> ResponseMetrics);
public record ResponseMetrics(int CountByStatus, double Rate, string Label, SecurityAnalysisStatus Status);

public static class TestRunnerResponseExtensions
{
    public static TestSummary CalculateTotals(this IEnumerable<TestRunnerResponse> testRunnerResponses)
    {
        var allStatuses = Enum.GetValues(typeof(SecurityAnalysisStatus))
                   .Cast<SecurityAnalysisStatus>()
                   .ToList();
        var allTestCases = testRunnerResponses.SelectMany(x => x.TestCaseResponses).ToList();
        var totalTestCasesRun = allTestCases.Count;
        var totalRulesRun = allTestCases.GroupBy(x => x.Rule.Name).Count();

        var metrics = new List<ResponseMetrics>();
        foreach (var status in allStatuses)
        {
            var countByStatus = allTestCases.Count(x => x.Status == status);
            var rate = (double)countByStatus / totalTestCasesRun * 100;

            metrics.Add(new ResponseMetrics(countByStatus, rate, $"Total {status.ToString()}:", status));
        }

        return new(totalRulesRun, totalTestCasesRun, metrics);
    }
}