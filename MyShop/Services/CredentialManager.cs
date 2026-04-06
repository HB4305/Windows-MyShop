using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Quản lý credentials đăng nhập và cấu hình database.
/// Lưu vào %APPDATA%\MyShop\config.json.
///
/// Bảo mật credentials:
///   - DPAPI (Windows): Mã hóa password trước khi lưu file. Chỉ user hiện tại
///     trên máy này mới giải mã được.
///   - SHA256 hash: Dùng để so sánh với DB mà không cần giải mã.
///   - Backward-compatible: Nếu config cũ lưu plain text (SavedPassword) vẫn đọc được.
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
    // Password helpers — mã hóa / giải mã / hash
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Băm SHA256 (base64). Dùng để so sánh với DB và lưu PasswordHash.
    /// </summary>
    public static string ComputeHash(string plaintext)
        => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(plaintext)));

    /// <summary>
    /// Mã hóa DPAPI — CurrentUser scope. Chỉ hỗ trợ trên Windows.
    /// Throws PlatformNotSupportedException trên các nền tảng khác.
    /// </summary>
    private static string EncryptDpapi(string plaintext)
    {
#if __WINDOWS__
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var encryptedBytes = ProtectedData.Protect(
            plaintextBytes,
            optionalEntropy: null,
            scope: DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encryptedBytes);
#else
        throw new PlatformNotSupportedException(
            "DPAPI is only supported on Windows. Credential storage requires Windows.");
#endif
    }

    /// <summary>
    /// Giải mã DPAPI. Chỉ hỗ trợ trên Windows.
    /// Ném ngoại lệ nếu không giải mã được (sai user / config hỏng).
    /// </summary>
    private static string DecryptDpapi(string encryptedBase64)
    {
#if __WINDOWS__
        var encryptedBytes = Convert.FromBase64String(encryptedBase64);
        var decryptedBytes = ProtectedData.Unprotect(
            encryptedBytes,
            optionalEntropy: null,
            scope: DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(decryptedBytes);
#else
        throw new PlatformNotSupportedException(
            "DPAPI is only supported on Windows. Credential storage requires Windows.");
#endif
    }

    /// <summary>
    /// Xác thực password thuần (plaintext) với hash đã lưu.
    /// </summary>
    private static bool VerifyPassword(string plaintext, string storedHash)
    {
        var hash = ComputeHash(plaintext);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(hash),
            Encoding.UTF8.GetBytes(storedHash));
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Credentials — Lưu / Đọc / Xóa
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Lưu email + password khi đăng nhập thành công.
    /// Password được mã hóa DPAPI + lưu SHA256 hash.
    /// </summary>
    public void SaveCredentials(string email, string password)
    {
        var config = LoadConfig();
        config.SavedEmail = email;
        config.PasswordHash = ComputeHash(password);

        try
        {
            config.EncryptedPassword = EncryptDpapi(password);
            config.SavedPassword = null; // Xóa plain text cũ
        }
        catch (PlatformNotSupportedException)
        {
            // Non-Windows platform: fallback lưu plain text
            config.EncryptedPassword = null;
            config.SavedPassword = password;
        }

        SaveConfig(config);
    }

    /// <summary>
    /// Xóa toàn bộ credentials. Gọi khi logout hoặc credentials hỏng.
    /// Vẫn giữ lại email để user không phải nhập lại.
    /// </summary>
    public void ClearCredentials()
    {
        var config = LoadConfig();
        config.SavedPassword = null;
        config.EncryptedPassword = null;
        config.PasswordHash = null;
        SaveConfig(config);
    }

    /// <summary>
    /// Xóa toàn bộ credentials BAO GỒM email.
    /// Dùng khi cần reset hoàn toàn (first-time setup).
    /// </summary>
    public void WipeAllCredentials()
    {
        var config = LoadConfig();
        config.SavedEmail = null;
        config.SavedPassword = null;
        config.EncryptedPassword = null;
        config.PasswordHash = null;
        SaveConfig(config);
    }

    /// <summary>
    /// Kiểm tra có credentials đã lưu chưa.
    /// </summary>
    public bool HasSavedCredentials()
    {
        var config = LoadConfig();
        return !string.IsNullOrWhiteSpace(config.SavedEmail)
            && (!string.IsNullOrWhiteSpace(config.PasswordHash)
                || !string.IsNullOrWhiteSpace(config.EncryptedPassword)
                || !string.IsNullOrWhiteSpace(config.SavedPassword));
    }

    /// <summary>
    /// Lấy password đã lưu (giải mã nếu có EncryptedPassword, fallback plain text).
    /// Trả về null nếu không có.
    /// </summary>
    public string? GetSavedPassword()
    {
        var config = LoadConfig();

        // Ưu tiên EncryptedPassword (DPAPI)
        if (!string.IsNullOrWhiteSpace(config.EncryptedPassword))
        {
            try { return DecryptDpapi(config.EncryptedPassword); }
            catch { /* config hỏng → fallback */ }
        }

        // Fallback: plain text cũ (backward compatible)
        return config.SavedPassword;
    }

    /// <summary>
    /// Lấy SHA256 hash của password đã lưu.
    /// Dùng cho LoginViewModel so sánh với DB mà không cần giải mã.
    /// </summary>
    public string? GetSavedPasswordHash()
        => LoadConfig().PasswordHash;

    /// <summary>
    /// Xác thực password thuần (plaintext) với hash đã lưu trong config.
    /// Dùng cho TryAutoLoginAsync — không cần truy cập DB.
    /// </summary>
    public bool ValidatePassword(string plaintext)
    {
        var config = LoadConfig();
        if (string.IsNullOrWhiteSpace(config.PasswordHash))
            return false;
        return VerifyPassword(plaintext, config.PasswordHash);
    }

    /// <summary>
    /// Kiểm tra xem password đã được lưu hash/encrypted chưa.
    /// Nếu chưa → cần lưu (first-time login).
    /// </summary>
    public bool NeedsCredentialMigration()
    {
        var config = LoadConfig();
        return string.IsNullOrWhiteSpace(config.PasswordHash)
            || string.IsNullOrWhiteSpace(config.EncryptedPassword);
    }

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
