using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Quản lý credentials đăng nhập và cấu hình database.
/// Lưu vào %APPDATA%\MyShop\config.json.
///
/// Bảo mật:
///   - DPAPI + Random Entropy: Mã hóa password trước khi lưu file.
///     Entropy ngẫu nhiên 20 bytes → mỗi lần encrypt cho kết quả khác nhau.
///     Chỉ user hiện tại trên máy này mới giải mã được.
///   - SHA256 hash: Dùng để so sánh với DB mà không cần giải mã.
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
    // DPAPI + Random Entropy — mã hóa / giải mã password cho config.json
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Mã hóa password bằng DPAPI với entropy ngẫu nhiên (20 bytes).
    /// Mỗi lần gọi → entropy KHÁC NHAU → kết quả mã hóa cũng khác nhau.
    /// Trả về: (encryptedPasswordBase64, entropyBase64).
    /// </summary>
    public static (string encrypted, string entropy) EncryptPassword(string plaintext)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(plaintext);

        // Tạo entropy ngẫu nhiên 20 bytes
        byte[] entropyBytes = new byte[20];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(entropyBytes);
        }

        var encryptedBytes = ProtectedData.Protect(
            passwordBytes,
            entropyBytes,
            DataProtectionScope.CurrentUser);

        return (
            Convert.ToBase64String(encryptedBytes),
            Convert.ToBase64String(entropyBytes)
        );
    }

    /// <summary>
    /// Giải mã password đã mã hóa bằng DPAPI + entropy.
    /// Trả về null nếu giải mã thất bại.
    /// </summary>
    public static string? DecryptPassword(string encryptedBase64, string entropyBase64)
    {
        try
        {
            var encryptedBytes = Convert.FromBase64String(encryptedBase64);
            var entropyBytes = Convert.FromBase64String(entropyBase64);
            var decryptedBytes = ProtectedData.Unprotect(
                encryptedBytes,
                entropyBytes,
                DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Băm SHA256 (Base64). Dùng để so sánh với DB.
    /// </summary>
    public static string ComputeHash(string plaintext)
        => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(plaintext)));

    // ═══════════════════════════════════════════════════════════════════════
    // Credentials — Lưu / Đọc / Xóa
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Lưu credentials khi đăng nhập thành công.
    /// Mã hóa password bằng DPAPI + random entropy.
    /// </summary>
    public void SaveCredentials(string email, string password, int userId, string userRole)
    {
        var config = LoadConfig();
        config.SavedEmail = email;
        config.PasswordHash = ComputeHash(password);
        config.CurrentUserId = userId;
        config.CurrentUserRole = userRole?.ToLowerInvariant();

        // Mã hóa password với entropy ngẫu nhiên
        var (encrypted, entropy) = EncryptPassword(password);
        config.EncryptedPassword = encrypted;
        config.PasswordEntropy = entropy;

        SaveConfig(config);
    }

    /// <summary>
    /// Xóa toàn bộ credentials. Gọi khi logout.
    /// Vẫn giữ lại email để user không phải nhập lại.
    /// </summary>
    public void ClearCredentials()
    {
        var config = LoadConfig();
        config.EncryptedPassword = null;
        config.PasswordEntropy = null;
        config.PasswordHash = null;
        SaveConfig(config);
    }

    /// <summary>
    /// Xóa toàn bộ credentials BAO GỒM email và user session.
    /// Dùng khi cần reset hoàn toàn.
    /// </summary>
    public void WipeAllCredentials()
    {
        var config = LoadConfig();
        config.SavedEmail = null;
        config.EncryptedPassword = null;
        config.PasswordEntropy = null;
        config.PasswordHash = null;
        config.CurrentUserId = null;
        config.CurrentUserRole = null;
        SaveConfig(config);
    }

    /// <summary>
    /// Kiểm tra có credentials đã lưu chưa.
    /// </summary>
    public bool HasSavedCredentials()
    {
        var config = LoadConfig();
        return !string.IsNullOrWhiteSpace(config.SavedEmail)
            && !string.IsNullOrWhiteSpace(config.EncryptedPassword)
            && !string.IsNullOrWhiteSpace(config.PasswordEntropy);
    }

    /// <summary>
    /// Lấy password đã lưu (giải mã DPAPI + entropy).
    /// Trả về null nếu không có hoặc giải mã thất bại.
    /// </summary>
    public string? GetSavedPassword()
    {
        var config = LoadConfig();
        if (string.IsNullOrWhiteSpace(config.EncryptedPassword)
            || string.IsNullOrWhiteSpace(config.PasswordEntropy))
            return null;

        return DecryptPassword(config.EncryptedPassword, config.PasswordEntropy);
    }

    /// <summary>
    /// Lấy hash SHA256 đã lưu (dùng để xác thực không qua DB).
    /// </summary>
    public string? GetSavedPasswordHash()
        => LoadConfig().PasswordHash;

    /// <summary>
    /// Lấy thông tin user đã lưu trong config.
    /// </summary>
    public (int? id, string? role) GetSavedUserInfo()
    {
        var config = LoadConfig();
        return (config.CurrentUserId, config.CurrentUserRole);
    }

    /// <summary>
    /// Xác thực password thuần (plaintext) với hash đã lưu trong config.
    /// Dùng cho auto-login — không cần DB.
    /// </summary>
    public bool ValidatePassword(string plaintext)
    {
        var config = LoadConfig();
        if (string.IsNullOrWhiteSpace(config.PasswordHash))
            return false;

        var hash = ComputeHash(plaintext);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(hash),
            Encoding.UTF8.GetBytes(config.PasswordHash));
    }

    /// <summary>
    /// Lấy email đã lưu.
    /// </summary>
    public string? GetSavedEmail()
        => LoadConfig().SavedEmail;

    // ═══════════════════════════════════════════════════════════════════════
    // Database Config
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
