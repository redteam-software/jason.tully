using RedTeamSecurityAnalyzer.TestExecution;

namespace RedTeamSecurityAnalyzer.Reporting;

public record GeneratedReport(string FilePath, string Name);

public interface ICommandReport<TReportData>
{
    public Task<GeneratedReport> GetGeneratedReportAsync(IEnumerable<TestRunnerResponse> Results, TReportData reportData, string reportTitle, string reportDirectory, CancellationToken cancellationToken = default);
}