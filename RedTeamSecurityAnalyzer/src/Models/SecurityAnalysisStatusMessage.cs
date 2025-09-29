namespace RedTeamSecurityAnalyzer.Models;

/// <summary>
/// Represents a message containing the status of a security analysis, including the number of failed and passed rules.
/// </summary>
/// <remarks>This struct provides a snapshot of the results of a security analysis, including the overall status
/// and the counts of failed and passed rules. It is intended to be used for reporting or logging the outcome of a
/// security analysis process.</remarks>
/// <param name="status"></param>
/// <param name="faiiledRules"></param>
/// <param name="passedRules"></param>
public struct SecurityAnalysisStatusMessage(SecurityAnalysisStatus status, double faiiledRules, double passedRules)
{
    public SecurityAnalysisStatus Status { get; } = status;
    public double FaiiledRules { get; } = faiiledRules;
    public double PassedRules { get; } = passedRules;
}