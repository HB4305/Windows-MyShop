using System.IO;

namespace MyShop.Utils;

/// <summary>
/// Simple utility to load .env files into Environment variables.
/// </summary>
public static class EnvLoader
{
    public static void Load()
    {
        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MyShop");
        Directory.CreateDirectory(appDataFolder);
        var appDataEnv = Path.Combine(appDataFolder, ".env");

        // Try to find .env in common locations (installed app, user app data, project root).
        var paths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, ".env"),
            appDataEnv,
            ".env",
            // Go up a few levels for IDE/Dev environments
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".env"),
            Path.Combine(Directory.GetCurrentDirectory(), ".env")
        };

        foreach (var path in paths)
        {
            if (File.Exists(path))
            {
                foreach (var line in File.ReadAllLines(path))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    var parts = line.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim();
                        var value = parts[1].Trim();
                        Environment.SetEnvironmentVariable(key, value);
                    }
                }
                break; // Stop after first successful load
            }
        }
    }
}
