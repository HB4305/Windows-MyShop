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

    // AES-256-GCM encrypted password (Base64). Stored as: [nonce(12)] [ciphertext+auth_tag(16)].
    public string? EncryptedPassword { get; set; }

    // SHA256 hash of plaintext password (Base64). Used for DB comparison
    // without needing to decrypt EncryptedPassword on every check.
    public string? PasswordHash { get; set; }

    // PBKDF2 salt (Base64, 16 bytes, random per encryption).
    // Combined with app constant salt to derive AES key for EncryptedPassword.
    public string? PasswordEntropy { get; set; }

    // Current logged-in user info (stored after successful login)
    public int? CurrentUserId { get; set; }
    public string? CurrentUserRole { get; set; }
}
