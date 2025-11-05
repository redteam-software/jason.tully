namespace RedTeamGoCli.Interfaces;

public record SshConfigEntry(string Host, Dictionary<string, string> Options);

public interface ISshConfigParser
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="filePath">if null, uses %USER_PROFILE/.ssh/</param>
    /// <returns></returns>
    public List<SshConfigEntry> Parse(string? filePath = null);

    /// <summary>
    ///
    /// </summary>
    /// <param name="entries"></param>
    /// <param name="filePath"></param>
    public void Save(List<SshConfigEntry> entries, string? filePath = null);
}