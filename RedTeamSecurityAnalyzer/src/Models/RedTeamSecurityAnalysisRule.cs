namespace RedTeamSecurityAnalyzer.Models;

/// <summary>
/// A security analysis rule.  A rule can have one or more test data patterns to test.
/// </summary>
/// <param name="Id"></param>
/// <param name="Name"></param>
/// <param name="Description"></param>
/// <param name="TestData"></param>
public record RedTeamSecurityAnalysisRule(int Id, string Name, string Description, string ReplacementToken, List<RedTeamSecurityAnalysisTestData> TestData, bool Enabled = true, bool RequiresAuthentication = false);
