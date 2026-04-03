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
    public string? SavedPassword { get; set; }
}
