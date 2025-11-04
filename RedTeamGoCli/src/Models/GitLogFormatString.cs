namespace RedTeamGoCli.Models;

public record GitLogFormatString(string Value)
{
    public static implicit operator GitLogFormatString(string value) => new GitLogFormatString(value);

    /// <summary>
    /// Returns commit information including hash, author, email, date, and message formatted as:
    /// </summary>
    public static GitLogFormatString CommitInfo => new GitLogFormatString("\"commit=%H|author=%an|email=%ae|date=%ai|message=%s|branch=%d\"");

    /// <summary>
    /// Returns only the sanitized commit message.
    /// </summary>
    public static GitLogFormatString Message => new GitLogFormatString("\"%s\"");
}