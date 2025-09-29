namespace RedTeamSecurityAnalyzer.Models;
public class FormDetails
{
    public string? FormSelector { get; set; }
    public string? FormName { get; set; }

    public string? FormSubmitButton { get; set; }

    public Dictionary<string, string> FormKeys { get; set; } = new Dictionary<string, string>();
}
