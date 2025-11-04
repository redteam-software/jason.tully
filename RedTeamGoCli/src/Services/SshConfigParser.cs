namespace RedTeamGoCli.Services;

[RegisterSingleton]
internal class SshConfigParser : ISshConfigParser
{
    private static string GetSshConfigPath(string? filePath = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(userProfile, ".ssh", "config");
        }

        return filePath;
    }
    public List<SshConfigEntry> Parse(string? filePath = null)
    {
        var entries = new List<SshConfigEntry>();
        SshConfigEntry? currentEntry = null;

        foreach (var rawLine in File.ReadLines(GetSshConfigPath(filePath)))
        {
            var line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;

            var parts = line.Split(new[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                continue;

            var key = parts[0];
            var value = parts[1];

            if (key.Equals("Host", StringComparison.OrdinalIgnoreCase))
            {
                currentEntry = new SshConfigEntry(value, new Dictionary<string, string>());
                entries.Add(currentEntry);
            }
            else if (currentEntry != null)
            {
                currentEntry.Options[key] = value;
            }
        }

        return entries;


    }

    public void Save(List<SshConfigEntry> entries, string? filePath = null)
    {
        using (var writer = new StringWriter())
        {
            foreach (var entry in entries)
            {
                writer.WriteLine($"Host {entry.Host}");
                foreach (var kvp in entry.Options)
                {
                    writer.WriteLine($"\t{kvp.Key} {kvp.Value}");
                }
                writer.WriteLine(); // Blank line between entries
            }

            Console.WriteLine(writer.GetStringBuilder().ToString());
        }


    }
}
