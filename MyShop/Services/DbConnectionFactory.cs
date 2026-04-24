using System.Data;
using MyShop.Models;
using Npgsql;

namespace MyShop.Services;

/// <summary>
/// Factory for creating NpgsqlConnections from DatabaseConfig.
/// Uses a singleton pattern - config only changes when the user saves in ConfigScreen.
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
    /// Gets the current connection string (cached in memory).
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
    /// Creates a new NpgsqlConnection.
    /// </summary>
    public NpgsqlConnection CreateConnection()
    {
        var connStr = GetConnectionString();
        return new NpgsqlConnection(connStr);
    }

    /// <summary>
    /// Creates a new IDbConnection (interface).
    /// </summary>
    public IDbConnection CreateDbConnection()
        => CreateConnection();

    /// <summary>
    /// Tests the connection with the provided config - returns null if OK, error message if failed.
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
                // Compatibility with Supabase Connection Pooler (Supavisor/PgBouncer transaction mode)
                MaxAutoPrepare = 0,
                NoResetOnClose = true,
            };
            await using var conn = new NpgsqlConnection(builder.ToString());
            await conn.OpenAsync();
            await conn.CloseAsync();
            return null; // success
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    /// <summary>
    /// Called after the user saves a new config - clears cache to rebuild.
    /// </summary>
    public void InvalidateCache()
    {
        _cachedConnectionString = null;
    }
}
