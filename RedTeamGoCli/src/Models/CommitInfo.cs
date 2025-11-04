namespace RedTeamGoCli.Models;

public record CommitInfo(

     string Message,
     string? Hash = null,
     string? Author = null,
     string? Email = null,
     DateTime? Date = null,
     HashSet<string>? ChangedFiles = null,
     string? Branch = null
 )
{
    public static CommitInfo Parse(string input)
    {
        var parts = input.Split('|', StringSplitOptions.RemoveEmptyEntries);

        var dict = parts.Select(p => p.Split('=', 2)).ToDictionary(kv => kv[0], kv => kv[1]);
        return new CommitInfo(
            Hash: dict["commit"],
            Author: dict["author"],
            Email: dict["email"],
            Date: DateTime.Parse(dict["date"]),
            Message: dict["message"],
            Branch: dict.ContainsKey("branch") ? dict["branch"] : null
            );
    }

    public string ToLocalTime()
    {
        if (Date != null)
        {
            return Date.Value.ToString("g");
        }
        return "-";
    }
}

