namespace RedTeamSecurityAnalyzer.Models;

public enum SecurityAnalysisStatus
{
    Passed = 200,
    Failed = 400,
    InternalServerError = 500,
    NotRun = 999,
    Initialized = 1000,
    Unknown = 10001
}