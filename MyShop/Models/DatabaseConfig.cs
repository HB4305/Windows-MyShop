using Npgsql;

namespace MyShop.Models;

/// <summary>
/// Five database configuration fields, shared between ConfigScreen and MauiProgram.
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
    /// Builds Npgsql connection string from the 5 fields.
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
            // Necessary when using Supabase Connection Pooler (Supavisor/PgBouncer transaction mode):
            // Poolers do not support prepared statements so they must be disabled.
            MaxAutoPrepare = 0,
            NoResetOnClose = true,
        };
        return builder.ToString();
    }

    /// <summary>
    /// Checks if all fields have been filled.
    /// </summary>
    public bool IsValid()
        => !string.IsNullOrWhiteSpace(Host)
            && Port > 0
            && !string.IsNullOrWhiteSpace(DatabaseName)
            && !string.IsNullOrWhiteSpace(Username);
}
