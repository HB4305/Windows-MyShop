using System.Text.Json;
using MyShop.Models.Ai;

namespace MyShop.Services;

public class AiLogService
{
    private readonly string _logPath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false
    };

    public AiLogService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var logDir = Path.Combine(appData, "MyShop", "AiLogs");
        Directory.CreateDirectory(logDir);
        _logPath = Path.Combine(logDir, "ai-log.jsonl");
    }

    public async Task LogAsync(AiLogEntry entry)
    {
        var json = JsonSerializer.Serialize(entry, _jsonOptions);
        await File.AppendAllTextAsync(_logPath, json + Environment.NewLine);
    }
}
