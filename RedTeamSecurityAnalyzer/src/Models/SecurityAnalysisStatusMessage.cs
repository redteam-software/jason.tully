namespace RedTeamSecurityAnalyzer.Models;

public struct SecurityAnalysisStatusMessage(SecurityAnalysisStatus status, double faiiledRules, double passedRules)
{
    public SecurityAnalysisStatus Status { get; } = status;
    public double FaiiledRules { get; } = faiiledRules;
    public double PassedRules { get; } = passedRules;
}