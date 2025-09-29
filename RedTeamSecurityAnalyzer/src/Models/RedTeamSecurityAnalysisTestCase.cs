using Newtonsoft.Json.Linq;

namespace RedTeamSecurityAnalyzer.Models;

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