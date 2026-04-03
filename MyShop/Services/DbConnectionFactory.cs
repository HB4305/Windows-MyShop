using System.Data;
using MyShop.Models;
using Npgsql;

namespace MyShop.Services;

/// <summary>
/// Factory tạo NpgsqlConnection từ DatabaseConfig.
/// Dùng singleton — config chỉ đổi khi user lưu ở ConfigScreen.
/// </summary>
public class DbConnectionFactory
{
    private readonly CredentialManager _credentialManager;
    private string? _cachedConnectionString;

    public DbConnectionFactory(CredentialManager credentialManager)
    {
        _credentialManager = credentialManager;
    }

    /// <summary>
    /// Lấy connection string hiện tại (cache trong memory).
    /// </summary>
    public string GetConnectionString()
    {
        if (!string.IsNullOrEmpty(_cachedConnectionString))
            return _cachedConnectionString;

        var (host, port, dbName, username, password) = _credentialManager.GetDatabaseConfig();
        Console.Error.WriteLine($"[DbConnFactory] GetConnectionString: host={host}, port={port}, db={dbName}, user={username}");
        var dbConfig = new DatabaseConfig(host ?? "", port, dbName ?? "", username ?? "", password ?? "");
        _cachedConnectionString = dbConfig.BuildConnectionString();
        Console.Error.WriteLine($"[DbConnFactory] Cached connString (no pwd): {_cachedConnectionString.Replace("Password=", "Password=***")}");
        return _cachedConnectionString;
    }

    /// <summary>
    /// Tạo mới một NpgsqlConnection.
    /// </summary>
    public NpgsqlConnection CreateConnection()
    {
        var connStr = GetConnectionString();
        return new NpgsqlConnection(connStr);
    }

    /// <summary>
    /// Tạo mới IDbConnection (interface).
    /// </summary>
    public IDbConnection CreateDbConnection()
        => CreateConnection();

    /// <summary>
    /// Test kết nối với config truyền vào — trả về null nếu OK, message lỗi nếu thất bại.
    /// </summary>
    public async Task<string?> TestConnectionAsync(string host, int port, string dbName, string username, string password)
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = host,
                Port = port,
                Database = dbName,
                Username = username,
                Password = password,
                Timeout = 10,
                // Tương thích với Supabase Connection Pooler (Supavisor/PgBouncer transaction mode)
                MaxAutoPrepare = 0,
                NoResetOnClose = true,
            };
            await using var conn = new NpgsqlConnection(builder.ToString());
            await conn.OpenAsync();
            await conn.CloseAsync();
            return null; // thành công
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    /// <summary>
    /// Gọi sau khi user lưu config mới — clear cache để rebuild.
    /// </summary>
    public void InvalidateCache()
    {
        _cachedConnectionString = null;
    }
}
