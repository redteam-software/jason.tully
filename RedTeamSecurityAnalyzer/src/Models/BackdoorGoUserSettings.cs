namespace RedTeamSecurityAnalyzer.Models;

/// <summary>
/// Encapsulates user settings for go's dev_logins page. 
/// </summary>
public class BackdoorGoUserSettings
{
    /// <summary>
    /// PCI
    /// </summary>
    public string DataAjax { get; set; } = null!;
    /// <summary>
    /// CompanyID
    /// </summary>
    public string DataId { get; set; } = null!;
    /// <summary>
    /// ContactId
    /// </summary>
    public string DataRel { get; set; } = null!;
}
