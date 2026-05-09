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
            SELECT id, email, password, COALESCE(role, 'sale'), created_at
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
                Role = reader.GetString(3),
                CreatedAt = reader.IsDBNull(4) ? null : reader.GetFieldValue<DateTimeOffset>(4)
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
            RETURNING id, email, role, created_at";

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
                    Role = reader.GetString(2),
                    CreatedAt = reader.IsDBNull(3) ? null : reader.GetFieldValue<DateTimeOffset>(3)
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

    public async Task<List<UserRecord>> GetAllAsync()
    {
        const string sql = @"
            SELECT id, email, COALESCE(role, 'sale'), created_at
            FROM users
            ORDER BY created_at DESC, id DESC";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);

        var users = new List<UserRecord>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            users.Add(new UserRecord
            {
                Id = reader.GetInt32(0),
                Email = reader.GetString(1),
                Role = reader.GetString(2),
                CreatedAt = reader.IsDBNull(3) ? null : reader.GetFieldValue<DateTimeOffset>(3)
            });
        }

        return users;
    }

    public async Task<UserRecord?> UpdateAsync(int id, string email, string role, string? password = null)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var normalizedRole = role?.Trim().ToLowerInvariant() ?? "sale";
        var updatePassword = !string.IsNullOrWhiteSpace(password);

        var sql = updatePassword
            ? @"
                UPDATE users
                SET email = @email,
                    role = @role,
                    password = @password
                WHERE id = @id
                RETURNING id, email, role, created_at"
            : @"
                UPDATE users
                SET email = @email,
                    role = @role
                WHERE id = @id
                RETURNING id, email, role, created_at";

        try
        {
            await using var conn = _connFactory.CreateConnection();
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);
            cmd.Parameters.AddWithValue("email", normalizedEmail);
            cmd.Parameters.AddWithValue("role", normalizedRole);
            if (updatePassword)
            {
                cmd.Parameters.AddWithValue("password", CredentialManager.ComputeHash(password!));
            }

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            return new UserRecord
            {
                Id = reader.GetInt32(0),
                Email = reader.GetString(1),
                Role = reader.GetString(2),
                CreatedAt = reader.IsDBNull(3) ? null : reader.GetFieldValue<DateTimeOffset>(3)
            };
        }
        catch (NpgsqlException)
        {
            return null;
        }
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM users WHERE id = @id";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> CountByRoleAsync(string role)
    {
        const string sql = @"
            SELECT COUNT(*)
            FROM users
            WHERE COALESCE(role, 'sale') = @role";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("role", role.Trim().ToLowerInvariant());

        var scalar = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(scalar);
    }
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
    public DateTimeOffset? CreatedAt { get; init; }
    public string RoleDisplay => (Role ?? "sale").ToUpperInvariant();
    public string CreatedAtDisplay => CreatedAt?.ToLocalTime().ToString("dd MMM yyyy HH:mm") ?? "-";
}
