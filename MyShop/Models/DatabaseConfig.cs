using Npgsql;

namespace MyShop.Models;

/// <summary>
/// 5 trường cấu hình database, dùng chung cho ConfigScreen và MauiProgram.
/// </summary>
public class DatabaseConfig
{
    public string Host { get; set; } = "localhost";
    public int    Port { get; set; } = 5432;
    public string DatabaseName { get; set; } = "myshop";
    public string Username { get; set; } = "postgres";
    public string Password { get; set; } = string.Empty;

    public DatabaseConfig() { }

    public DatabaseConfig(string host, int port, string dbName, string username, string password)
    {
        Host = host;
        Port = port;
        DatabaseName = dbName;
        Username = username;
        Password = password;
    }

    /// <summary>
    /// Build Npgsql connection string từ 5 trường.
    /// </summary>
    public string BuildConnectionString()
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = Host,
            Port = Port,
            Database = DatabaseName,
            Username = Username,
            Password = Password,
            Timeout = 10,
            // Cần thiết khi dùng Supabase Connection Pooler (Supavisor/PgBouncer transaction mode):
            // Pooler không hỗ trợ prepared statements nên phải tắt.
            MaxAutoPrepare = 0,
            NoResetOnClose = true,
        };
        return builder.ToString();
    }

    /// <summary>
    /// Kiểm tra tất cả trường đã được điền chưa.
    /// </summary>
    public bool IsValid()
        => !string.IsNullOrWhiteSpace(Host)
            && Port > 0
            && !string.IsNullOrWhiteSpace(DatabaseName)
            && !string.IsNullOrWhiteSpace(Username);
}
