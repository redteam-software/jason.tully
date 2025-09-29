using Newtonsoft.Json.Linq;

namespace RedTeamSecurityAnalyzer.Models;

/// <summary>
/// Actual data to be feed into the test runner.
/// </summary>
/// <param name="Pattern"></param>
/// <param name="Description"></param>
public record RedTeamSecurityAnalysisTestData(string Pattern, string Description, int[] SuccessStatusCodes, int[] FailureStatusCodes);
/// <summary>
/// A security analysis rule.  A rule can have one or more test data patterns to test.
/// </summary>
/// <param name="Id"></param>
/// <param name="Name"></param>
/// <param name="Description"></param>
/// <param name="TestData"></param>
public record RedTeamSecurityAnalysisRule(int Id, string Name, string Description, string ReplacementToken, List<RedTeamSecurityAnalysisTestData> TestData, bool Enabled = true, bool RequiresAuthentication = false);

/// <summary>
///  A security analysis test case.  A test case can have one or more rules to test.
/// </summary>
/// <param name="Name"></param>
/// <param name="BaseUrl"></param>
/// <param name="Rules"></param>
public record RedTeamSecurityAnalysisTestCase(string Name,
    string BaseUrl,
    string TestRunner,
    string? Category,
    List<RedTeamSecurityAnalysisRule> Rules,
    Dictionary<string, object> Properties,
    bool Enabled = true,
    bool RequiresAuthentication = false)
{
    public TData? PropertiesTo<TData>()
    {
        return JObject.FromObject(Properties).ToObject<TData>();
    }
}