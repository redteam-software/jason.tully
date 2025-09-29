namespace RedTeamSecurityAnalyzer.Models;
/// <summary>
/// Models details about a web form, including its selector, name, submit button, and key-value pairs for form fields.
/// </summary>
public class TestCaseProperties
{
    /// <summary>
    /// The form selector (CSS selector or XPath) to identify the form on a web page.
    /// </summary>
    public string? FormSelector { get; set; }
    /// <summary>
    /// The name of the form
    /// </summary>
    public string? FormName { get; set; }
    /// <summary>
    /// The form button selector (CSS selector or XPath) to identify the submit button on the form.
    /// </summary>
    public string? FormSubmitButton { get; set; }

    /// <summary>
    ///  Form keys and their corresponding values to be filled in the form fields.
    /// </summary>
    public Dictionary<string, string> FormKeys { get; set; } = new Dictionary<string, string>();
    /// <summary>
    /// PCI
    /// </summary>
    public string? DataAjax { get; set; }
    /// <summary>
    /// CompanyID
    /// </summary>
    public string? DataId { get; set; }
    /// <summary>
    /// ContactId
    /// </summary>
    public string? DataRel { get; set; }
}
