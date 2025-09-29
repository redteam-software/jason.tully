namespace RedTeamSecurityAnalyzer.Models;

/// <summary>
/// Actual data to be feed into the test runner.
/// </summary>
/// <param name="Pattern"></param>
/// <param name="Description"></param>
public record RedTeamSecurityAnalysisTestData(string Pattern, string Description, int[] SuccessStatusCodes, int[] FailureStatusCodes);
