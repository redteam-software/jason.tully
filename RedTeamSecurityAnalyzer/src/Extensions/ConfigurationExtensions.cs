using Microsoft.Extensions.Configuration;

namespace RedTeamSecurityAnalyzer.Extensions;
public static class ConfigurationExtensions
{
    /// <summary>
    /// Adds the application and rule configuration files to the configuration manager.
    /// </summary>
    /// <param name="configurationManager"></param>
    /// <returns></returns>
    public static IConfigurationManager AddApplicationAndRuleConfiguration(this IConfigurationManager configurationManager)
    {
        var applicationFilePath = @"./Data/ApplicationTestCases.json";
        var ruleFilePath = @"./Data/RuleDefinitions.json";
        configurationManager.AddJsonFile(applicationFilePath, optional: false, reloadOnChange: true);
        configurationManager.AddJsonFile(ruleFilePath, optional: false, reloadOnChange: true);
        return configurationManager;
    }
}
