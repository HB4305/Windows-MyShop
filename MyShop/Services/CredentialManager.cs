using System.Text.Json;
using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Quản lý credentials đăng nhập và cấu hình database.
/// Lưu vào %APPDATA%\MyShop\config.json.
/// Works trên mọi nền tảng: Windows, macOS, Linux, Android, iOS, WASM.
/// </summary>
public class CredentialManager
{
    private readonly string _configPath;

    public CredentialManager()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appData, "MyShop");
        Directory.CreateDirectory(appFolder);
        _configPath = Path.Combine(appFolder, "config.json");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Credentials — Lưu / Đọc
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Lưu email + password khi đăng nhập thành công.
    /// </summary>
    public void SaveCredentials(string email, string password)
    {
        var config = LoadConfig();
        config.SavedEmail = email;
        config.SavedPassword = password;
        SaveConfig(config);
    }

    /// <summary>
    /// Xóa credentials. Gọi khi đăng xuất hoặc credentials hỏng.
    /// </summary>
    public void ClearCredentials()
    {
        var config = LoadConfig();
        config.SavedEmail = null;
        config.SavedPassword = null;
        SaveConfig(config);
    }

    /// <summary>
    /// Kiểm tra có credentials đã lưu chưa.
    /// </summary>
    public bool HasSavedCredentials()
    {
        var config = LoadConfig();
        return !string.IsNullOrWhiteSpace(config.SavedEmail)
            && !string.IsNullOrWhiteSpace(config.SavedPassword);
    }

    /// <summary>
    /// Lấy password đã lưu.
    /// </summary>
    public string? GetSavedPassword()
        => LoadConfig().SavedPassword;

    /// <summary>
    /// Lấy email đã lưu.
    /// </summary>
    public string? GetSavedEmail()
        => LoadConfig().SavedEmail;

    // ═══════════════════════════════════════════════════════════════════════
    // Database Config — 5 trường: Host, Port, DatabaseName, Username, Password
    // ═══════════════════════════════════════════════════════════════════════

    public (string? host, int port, string? dbName, string? username, string? password) GetDatabaseConfig()
    {
        var config = LoadConfig();
        return (config.DbHost, config.DbPort, config.DbName, config.DbUsername, config.DbPassword);
    }

    public void SaveDatabaseConfig(string host, int port, string dbName, string username, string password)
    {
        var config = LoadConfig();
        config.DbHost = host;
        config.DbPort = port;
        config.DbName = dbName;
        config.DbUsername = username;
        config.DbPassword = password;
        SaveConfig(config);
    }

    /// <summary>
    /// Kiểm tra đã có đủ config chưa.
    /// </summary>
    public bool HasDatabaseConfig()
    {
        var (host, port, dbName, username, password) = GetDatabaseConfig();
        return !string.IsNullOrWhiteSpace(host)
            && port > 0
            && !string.IsNullOrWhiteSpace(dbName)
            && !string.IsNullOrWhiteSpace(username)
            && !string.IsNullOrWhiteSpace(password);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // File I/O
    // ═══════════════════════════════════════════════════════════════════════

    private AppConfig LoadConfig()
    {
        if (!File.Exists(_configPath))
            return new AppConfig();

        try
        {
            var json = File.ReadAllText(_configPath);
            return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }
        catch
        {
            return new AppConfig();
        }
    }

    private void SaveConfig(AppConfig config)
    {
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_configPath, json);
    }
}
