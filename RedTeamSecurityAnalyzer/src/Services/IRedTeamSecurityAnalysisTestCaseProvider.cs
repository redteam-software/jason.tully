using Newtonsoft.Json;
using RedTeamSecurityAnalyzer.Configuration;

namespace RedTeamSecurityAnalyzer.Services;

public interface IRedTeamSecurityAnalysisTestCaseProvider
{
    /// <summary>
    /// Configures all tests cases for the specified application.
    /// </summary>
    /// <param name="redTeamApplication"></param>
    /// <returns></returns>
    IEnumerable<RedTeamSecurityAnalysisTestCase> RedTeamSecurityAnalysisTestCases(RedTeamApplication redTeamApplication);
}

public static class IRedTeamSecurityAnalysisTestCaseProviderExtensions
{
    /// <summary>
    /// Configures all tests cases for the specified application.
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="application"></param>
    /// <returns></returns>
    public static IEnumerable<RedTeamSecurityAnalysisTestCase> RedTeamSecurityAnalysisTestCases(this IRedTeamSecurityAnalysisTestCaseProvider provider, string application)
    {
        return provider.RedTeamSecurityAnalysisTestCases(RedTeamApplicationUtil.Parse(application)).ToList();
    }
}

[RegisterSingleton<IRedTeamSecurityAnalysisTestCaseProvider>]
public class RedTeamSecurityAnalysisTestCaseProvider : IRedTeamSecurityAnalysisTestCaseProvider
{
    private readonly SecurityAnalysisTestCasesConfiguration _securityAnalysisTestCases;

    public RedTeamSecurityAnalysisTestCaseProvider()
    {
        _securityAnalysisTestCases = JsonConvert.DeserializeObject<SecurityAnalysisTestCasesConfiguration>(File.ReadAllText(@"./Rules/RuleDefinitions.json"))!;
    }

    public IEnumerable<RedTeamSecurityAnalysisTestCase> RedTeamSecurityAnalysisTestCases(RedTeamApplication redTeamApplication)
    {
        var application = _securityAnalysisTestCases.Applications[redTeamApplication.ToString()]!;

        var query = from test in application.Tests.Where(x => x.Enabled)
                    from ruleName in test.Rules
                    join rule in _securityAnalysisTestCases.Rules on ruleName equals rule.Name into ruleGroup
                    select new RedTeamSecurityAnalysisTestCase(test.Name,
                    test.BaseUrl,
                    test.Runner,
                    test.Category,
                    MapRules(ruleGroup),
                    test.Properties,
                    test.Enabled,
                    test.RequiresAuthentication);

        return query.ToList();
    }

    List<RedTeamSecurityAnalysisRule> MapRules(IEnumerable<SecurityAnalysisRuleConfiguration> ruleConfiguration)
    {

        //find any references to other rules and pull them in.

        var testsDataWithReferences = ruleConfiguration.Where(x => x.TestData.Any(x => x.ReferencesRules != null && x.ReferencesRules.Any()));

        var withReferences = from refs in testsDataWithReferences
                             from td in refs.TestData
                             from r in td.ReferencesRules
                             select MapReferencedRules(r);



        var withoutReferences = ruleConfiguration.Where(x => x.TestData.All(x => x.ReferencesRules == null || !x.ReferencesRules.Any()))
            .Select(r => new RedTeamSecurityAnalysisRule(
            r.ID,
            r.Name,
            r.Description,
            r.ReplacementToken,
            Map(r.TestData),
            r.Enabled,
            r.RequiresAuthentication
        ));

        if (withReferences.Any())
        {

            //combine into one list 
            var combinded = withoutReferences.Concat(withReferences).ToList();

            var rule = new RedTeamSecurityAnalysisRule(Int32.MaxValue,
                "Aggregated",
                String.Join("/", combinded.Select(x => x.Name).Distinct()), combinded.First().ReplacementToken, combinded.SelectMany(x => x.TestData).ToList(), true, true);

            return [rule];
        }


        return withoutReferences.ToList();
    }

    List<RedTeamSecurityAnalysisTestData> Map(IEnumerable<TestData> tests)
    {


        return tests.Select(td => new RedTeamSecurityAnalysisTestData(td.Pattern, td.Description, td.SuccessStatusCodes, td.FailureStatusCodes)).ToList();
    }

    private RedTeamSecurityAnalysisRule MapReferencedRules(string ruleName)
    {
        var rules = _securityAnalysisTestCases.Rules.Where(x => x.Name == ruleName);
        return MapRules(rules).First();
    }
}