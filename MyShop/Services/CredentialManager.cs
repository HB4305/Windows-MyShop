using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Manages login credentials and database configuration.
/// File: %APPDATA%\MyShop\config.json (macOS: ~/Library/Application Support/MyShop/config.json)
///
/// Security:
///   - AES-256-GCM: Cross-platform authenticated encryption.
///     A random 16-byte salt is derived into a key using PBKDF2 (100k iterations).
///     Each encryption produces different ciphertext (random IV per call).
///   - SHA256 hash: Used for DB comparison without decryption.
/// </summary>
public partial class CredentialManager
{
    // Application-specific constant salt for PBKDF2 key derivation.
    // Combined with the per-encryption random entropy, this gives strong
    // key material that never leaves the machine unencrypted.
    private const string AppSalt = "MyShop_v1_credential_salt_do_not_change";

    private readonly string _configPath;

    public CredentialManager()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appData, "MyShop");
        Directory.CreateDirectory(appFolder);
        _configPath = Path.Combine(appFolder, "config.json");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // AES-256-GCM — cross-platform encryption / decryption
    // Replaces Windows-only DPAPI (ProtectedData)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Encrypts a plaintext password using AES-256-GCM with PBKDF2 key derivation.
    /// Returns (encryptedBase64, entropyBase64).
    /// Each call produces a different result because a random salt is used each time.
    /// </summary>
    public static (string encrypted, string entropy) EncryptPassword(string plaintext)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(plaintext);

        // Random 16-byte salt — unique per encryption, stored alongside ciphertext
        byte[] saltBytes = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }

        // Derive AES-256 key from AppSalt + saltBytes using PBKDF2
        byte[] key = DeriveKey(AppSalt, saltBytes);

        // Random 12-byte nonce (IV) for AES-GCM
        byte[] nonce = new byte[12];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(nonce);
        }

        // Encrypt with AES-256-GCM (authenticated encryption)
        var cipherText = AesGcmEncrypt(passwordBytes, key, nonce);

        // Combine: [nonce (12)] [ciphertext+tag] — stored as Base64 string
        byte[] combined = new byte[nonce.Length + cipherText.Length];
        Buffer.BlockCopy(nonce, 0, combined, 0, nonce.Length);
        Buffer.BlockCopy(cipherText, 0, combined, nonce.Length, cipherText.Length);

        return (
            Convert.ToBase64String(combined),
            Convert.ToBase64String(saltBytes)
        );
    }

    /// <summary>
    /// Decrypts a password encrypted with AES-256-GCM.
    /// Returns null if decryption fails.
    /// </summary>
    public static string? DecryptPassword(string encryptedBase64, string entropyBase64)
    {
        try
        {
            var combined = Convert.FromBase64String(encryptedBase64);
            var saltBytes = Convert.FromBase64String(entropyBase64);

            // Extract nonce (first 12 bytes) and ciphertext
            byte[] nonce = new byte[12];
            byte[] cipherText = new byte[combined.Length - nonce.Length];
            Buffer.BlockCopy(combined, 0, nonce, 0, nonce.Length);
            Buffer.BlockCopy(combined, nonce.Length, cipherText, 0, cipherText.Length);

            // Re-derive key
            byte[] key = DeriveKey(AppSalt, saltBytes);

            byte[] decryptedBytes = AesGcmDecrypt(cipherText, key, nonce);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Derives a 256-bit AES key from the app salt + per-encryption salt using PBKDF2.
    /// 100,000 iterations — computationally expensive to brute-force.
    /// </summary>
    private static byte[] DeriveKey(string appSalt, byte[] saltBytes)
    {
        byte[] combined = Encoding.UTF8.GetBytes(appSalt);
        byte[] input = new byte[combined.Length + saltBytes.Length];
        Buffer.BlockCopy(combined, 0, input, 0, combined.Length);
        Buffer.BlockCopy(saltBytes, 0, input, combined.Length, saltBytes.Length);

        // Static Pbkdf2 is the recommended non-obsolete API (SYSLIB0060)
        return Rfc2898DeriveBytes.Pbkdf2(
            input,
            saltBytes,
            100_000,
            HashAlgorithmName.SHA256,
            32);
    }

    /// <summary>
    /// Encrypts plaintext using AES-256-GCM. Returns ciphertext + 16-byte auth tag.
    /// </summary>
    private static byte[] AesGcmEncrypt(byte[] plaintext, byte[] key, byte[] nonce)
    {
        byte[] cipherText = new byte[plaintext.Length];
        byte[] tag = new byte[16];
        using var aes = new AesGcm(key);
        aes.Encrypt(nonce, plaintext, cipherText, tag);

        // Combine ciphertext + tag (tag needed for decryption)
        byte[] result = new byte[cipherText.Length + tag.Length];
        Buffer.BlockCopy(cipherText, 0, result, 0, cipherText.Length);
        Buffer.BlockCopy(tag, 0, result, cipherText.Length, tag.Length);
        return result;
    }

    /// <summary>
    /// Decrypts AES-256-GCM ciphertext (ciphertext + 16-byte auth tag). Throws on auth failure.
    /// </summary>
    private static byte[] AesGcmDecrypt(byte[] cipherTextWithTag, byte[] key, byte[] nonce)
    {
        int tagLen = 16;
        byte[] cipherText = new byte[cipherTextWithTag.Length - tagLen];
        byte[] tag = new byte[tagLen];
        Buffer.BlockCopy(cipherTextWithTag, 0, cipherText, 0, cipherText.Length);
        Buffer.BlockCopy(cipherTextWithTag, cipherText.Length, tag, 0, tagLen);

        byte[] plaintext = new byte[cipherText.Length];
        using var aes = new AesGcm(key);
        aes.Decrypt(nonce, cipherText, tag, plaintext);
        return plaintext;
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
    /// Saves credentials after successful login.
    /// AES-256-GCM encryption with per-encryption random salt.
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
    /// Retrieves the stored password (AES-256-GCM decryption).
    /// Returns null if not present or decryption fails.
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
