namespace RedTeamSecurityAnalyzer.Configuration;

public class RedTeamApplicationConfiguration
{
    public List<SecurityAnalysisTestConfiguration> Tests { get; set; } = new List<SecurityAnalysisTestConfiguration>();
}

public class SecurityAnalysisTestCasesConfiguration
{
    public Dictionary<string, RedTeamApplicationConfiguration> Applications { get; set; } = new Dictionary<string, RedTeamApplicationConfiguration>();
    public List<SecurityAnalysisRuleConfiguration> Rules { get; set; } = new List<SecurityAnalysisRuleConfiguration>();
}

public class SecurityAnalysisTestConfiguration
{
    public string Name { get; set; } = null!;
    public string BaseUrl { get; set; } = null!;

    public string Runner { get; set; } = "HttpGet";
    public string? Category { get; set; }

    public bool Enabled { get; set; } = true;
    public bool RequiresAuthentication { get; set; } = false;
    public List<string> Rules { get; set; } = new List<string>();

    public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
}

public class TestData
{
    public string Pattern { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int[] SuccessStatusCodes { get; set; } = null!;
    public int[] FailureStatusCodes { get; set; } = null!;

    public List<string> ReferencesRules { get; set; } = new List<string>();
}

public class SecurityAnalysisRuleConfiguration
{
    public string Description { get; set; } = null!;
    public int ID { get; init; }
    public string Name { get; set; } = null!;

    public string ReplacementToken { get; set; } = "{pattern}";
    public List<TestData> TestData { get; set; } = null!;

    public bool Enabled { get; set; } = true;

    public bool RequiresAuthentication { get; set; } = false;
}