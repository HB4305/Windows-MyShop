namespace MyShop.Models;

/// <summary>
/// Lưu cấu hình và credentials.
/// File: %APPDATA%\MyShop\config.json
/// </summary>
public class AppConfig
{
    // Database Config (ConfigScreen — 5 trường)
    public string? DbHost { get; set; }
    public int    DbPort { get; set; }
    public string? DbName { get; set; }
    public string? DbUsername { get; set; }
    public string? DbPassword { get; set; }

    // Credentials (Login — lưu sau khi đăng nhập thành công)
    public string? SavedEmail { get; set; }

    // Plain text — kept for migration from old config, prefer EncryptedPassword
    public string? SavedPassword { get; set; }

    // DPAPI-encrypted password (base64). Recommended over SavedPassword.
    public string? EncryptedPassword { get; set; }

    // SHA256 hash of plaintext password (base64). Used for DB comparison
    // without needing to decrypt EncryptedPassword on every check.
    public string? PasswordHash { get; set; }

    // DPAPI entropy (Base64, 20 bytes, random per encryption).
    // Used to decrypt EncryptedPassword stored in config.json.
    public string? PasswordEntropy { get; set; }

    // Current logged-in user info (stored after successful login)
    public int? CurrentUserId { get; set; }
    public string? CurrentUserRole { get; set; }
}
