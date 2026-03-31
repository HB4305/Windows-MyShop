namespace MyShop.Models;

/// <summary>
/// Lưu cấu hình và credentials.
/// File: %APPDATA%\MyShop\config.json
/// </summary>
public class AppConfig
{
    // Server Config (ConfigScreen)
    public string? SupabaseUrl { get; set; }
    public string? SupabaseAnonKey { get; set; }

    // Credentials (Login)
    public string? SavedEmail { get; set; }
    public string? BcryptHash { get; set; }

    // JWT Tokens (DPAPI-mã hóa)
    public string? EncryptedAccessToken { get; set; }
    public string? EncryptedRefreshToken { get; set; }
    public long? TokenExpiry { get; set; }
}
