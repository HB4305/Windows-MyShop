using System.Text;
using System.Text.Json;
using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Quản lý credentials đăng nhập.
/// - Lưu JWT Access Token (base64-encoded) vào config.json trong thư mục app data
/// - BCrypt hash password cho việc verify local (không gửi password qua network)
/// Works trên mọi nền tảng: Windows, macOS, Linux, Android, iOS, WebAssembly.
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
    // BCrypt — Mã hóa & Verify mật khẩu
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Hash mật khẩu bằng BCrypt (cost factor mặc định = 11).
    /// Dùng khi tạo tài khoản owner lần đầu.
    /// </summary>
    public static string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);

    /// <summary>
    /// Verify mật khẩu với BCrypt hash đã lưu.
    /// Trả về true nếu khớp.
    /// </summary>
    public static bool VerifyPassword(string password, string bcryptHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, bcryptHash);
        }
        catch
        {
            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // JWT Token — Lưu / Đọc
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Lưu JWT tokens + BCrypt hash + email.
    /// Gọi sau khi đăng nhập thành công.
    /// </summary>
    public void SaveCredentials(string email, string bcryptHash, string accessToken, string refreshToken)
    {
        var config = LoadConfig();
        config.SavedEmail = email;
        config.BcryptHash = bcryptHash;
        config.EncryptedAccessToken = Base64Encode(accessToken);
        config.EncryptedRefreshToken = Base64Encode(refreshToken);
        config.TokenExpiry = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds();
        SaveConfig(config);
    }

    /// <summary>
    /// Xóa credentials. Gọi khi đăng xuất hoặc credentials hỏng.
    /// </summary>
    public void ClearCredentials()
    {
        var config = LoadConfig();
        config.SavedEmail = null;
        config.BcryptHash = null;
        config.EncryptedAccessToken = null;
        config.EncryptedRefreshToken = null;
        config.TokenExpiry = null;
        SaveConfig(config);
    }

    /// <summary>
    /// Đọc JWT tokens đã lưu. Trả về null nếu hết hạn hoặc không có.
    /// </summary>
    public (string accessToken, string refreshToken)? LoadTokens()
    {
        var config = LoadConfig();
        if (string.IsNullOrEmpty(config.EncryptedAccessToken))
            return null;

        // Kiểm tra expiry
        if (config.TokenExpiry.HasValue &&
            DateTimeOffset.UtcNow.ToUnixTimeSeconds() > config.TokenExpiry.Value)
        {
            ClearCredentials();
            return null;
        }

        try
        {
            var accessToken = Base64Decode(config.EncryptedAccessToken!);
            var refreshToken = config.EncryptedRefreshToken != null
                ? Base64Decode(config.EncryptedRefreshToken)
                : "";
            return (accessToken, refreshToken);
        }
        catch
        {
            ClearCredentials();
            return null;
        }
    }

    /// <summary>
    /// Kiểm tra có JWT Token đã lưu và còn hạn không.
    /// </summary>
    public bool HasValidToken()
    {
        var tokens = LoadTokens();
        return tokens.HasValue;
    }

    /// <summary>
    /// Lấy BCrypt hash đã lưu.
    /// </summary>
    public string? GetBcryptHash()
        => LoadConfig().BcryptHash;

    /// <summary>
    /// Lấy email đã lưu.
    /// </summary>
    public string? GetSavedEmail()
        => LoadConfig().SavedEmail;

    // ═══════════════════════════════════════════════════════════════════════
    // Server Config
    // ═══════════════════════════════════════════════════════════════════════

    public string? GetSupabaseUrl()
        => LoadConfig().SupabaseUrl;

    public string? GetSupabaseAnonKey()
        => LoadConfig().SupabaseAnonKey;

    public void SaveServerConfig(string url, string anonKey)
    {
        var config = LoadConfig();
        config.SupabaseUrl = url;
        config.SupabaseAnonKey = anonKey;
        SaveConfig(config);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Token Persistence — Cross-platform, đa nền tảng
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Lưu tokens dạng plain base64 (không mã hóa).
    /// Không cần DPAPI vì:
    ///   1. JWT chỉ có giá trị khi được server Supabase chấp nhận
    ///   2. Supabase Client tự validate & refresh token
    ///   3. User luôn có thể đăng nhập lại nếu token bị đánh cắp
    /// Works trên mọi nền tảng: Windows, macOS, Linux, Android, iOS, WASM.
    /// </summary>
    private static string Base64Encode(string data)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(data));

    /// <summary>
    /// Giải mã base64.
    /// </summary>
    private static string Base64Decode(string base64)
        => Encoding.UTF8.GetString(Convert.FromBase64String(base64));

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
