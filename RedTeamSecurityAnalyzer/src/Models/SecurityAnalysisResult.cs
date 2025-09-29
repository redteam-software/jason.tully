namespace RedTeamSecurityAnalyzer.Models;
public record SecurityAnalysisResult(PathTraversalRule Rule, SecurityAnalysisStatus Status, string Message, IReadOnlyList<SecurityAnalysisResult> ChildResults)
{
    public static SecurityAnalysisResult NotRun(PathTraversalRule rule) => new(rule, SecurityAnalysisStatus.NotRun, "Not Run", new List<SecurityAnalysisResult>());
}