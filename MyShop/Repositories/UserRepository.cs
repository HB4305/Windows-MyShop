using MyShop.Models;
using MyShop.Services;
using Npgsql;

namespace MyShop.Repositories;

/// <summary>
/// Repository for the users table.
/// Used for login and retrieving user info by email.
/// </summary>
public class UserRepository
{
    private readonly DbConnectionFactory _connFactory;

    public UserRepository(DbConnectionFactory connFactory) => _connFactory = connFactory;

    /// <summary>
    /// Finds a user by email. Returns null if not found.
    /// </summary>
    public async Task<UserRecord?> GetByEmailAsync(string email)
    {
        const string sql = @"
            SELECT id, email, password, COALESCE(role, 'sale')
            FROM users
            WHERE email = @email
            LIMIT 1";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("email", email.Trim().ToLowerInvariant());

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new UserRecord
            {
                Id = reader.GetInt32(0),
                Email = reader.GetString(1),
                Password = reader.IsDBNull(2) ? "" : reader.GetString(2),
                Role = reader.GetString(3)
            };
        }

        return null;
    }

    /// <summary>
    /// Creates a new user (register owner or sale).
    /// Password is SHA256 hashed before being stored in the database.
    /// Returns UserRecord if successful, null if failed.
    /// </summary>
    public async Task<UserRecord?> CreateAsync(string email, string password, string role)
    {
        var hashedPassword = CredentialManager.ComputeHash(password);

        const string sql = @"
            INSERT INTO users (email, password, role)
            VALUES (@email, @password, @role)
            RETURNING id, email, role";

        try
        {
            await using var conn = _connFactory.CreateConnection();
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("email", email.Trim().ToLowerInvariant());
            cmd.Parameters.AddWithValue("password", hashedPassword);
            cmd.Parameters.AddWithValue("role", role?.ToLowerInvariant() ?? "sale");

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new UserRecord
                {
                    Id = reader.GetInt32(0),
                    Email = reader.GetString(1),
                    Password = hashedPassword,
                    Role = reader.GetString(2)
                };
            }
            return null;
        }
        catch (NpgsqlException)
        {
            return null; // email already exists or other error
        }
    }

    /// <summary>
    /// Checks if any users exist in the system.
    /// </summary>
    public async Task<bool> HasAnyUserAsync()
    {
        const string sql = "SELECT EXISTS(SELECT 1 FROM users LIMIT 1)";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        var result = await cmd.ExecuteScalarAsync();
        return result is bool b && b;
    }

    /// <summary>
    /// Creates the initial owner.
    /// </summary>
    public async Task<UserRecord?> CreateOwnerAsync(string email, string password)
        => await CreateAsync(email, password, "owner");
}

/// <summary>
/// DTO for the users table.
/// </summary>
public record UserRecord
{
    public int Id { get; init; }
    public string Email { get; init; } = string.Empty;
    /// <summary>SHA256 hash of the password (Base64).</summary>
    public string Password { get; init; } = string.Empty;
    public string? Role { get; init; }
}
