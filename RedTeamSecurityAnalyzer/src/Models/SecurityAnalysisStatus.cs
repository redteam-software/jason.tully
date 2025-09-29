namespace RedTeamSecurityAnalyzer.Models;

/// <summary>
/// Test result status codes for security analysis.
/// </summary>
public enum SecurityAnalysisStatus
{
    Passed = 200,
    Failed = 400,
    InternalServerError = 500,
    NotRun = 999,
    Initialized = 1000,
    Unknown = 10001
}