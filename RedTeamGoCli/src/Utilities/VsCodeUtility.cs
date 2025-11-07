namespace RedTeamGoCli.Utilities;
using System;
using System.Diagnostics;

public static class VsCodeUtility
{
    /// <summary>
    /// Opens the specified file in Visual Studio Code.
    /// </summary>
    /// <param name="filePath">Full path to the file to open.</param>
    public static void OpenFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            Console.WriteLine("File path is empty or null.");
            return;
        }

        if (!System.IO.File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return;
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "code", // Assumes 'code' is in PATH
                Arguments = $"\"{filePath}\"",
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to open file in VS Code: {ex.Message}");
        }
    }
}
