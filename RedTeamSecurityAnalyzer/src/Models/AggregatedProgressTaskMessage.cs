using RedTeam.Extensions.Console.Models;

namespace RedTeamSecurityAnalyzer.Models;
/// <summary>
/// An aggregated message containing both progress task and security analysis status.
/// </summary>
/// <param name="ProgressTaskMessage"></param>
/// <param name="SecurityAnalysisStatusMessag"></param>
public record AggregatedProgressTaskMessage(ProgressTaskMessage ProgressTaskMessage, SecurityAnalysisStatusMessage SecurityAnalysisStatusMessag);