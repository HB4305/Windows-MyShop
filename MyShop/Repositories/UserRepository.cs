using MyShop.Models;
using MyShop.Services;
using Npgsql;

namespace MyShop.Repositories;

/// <summary>
/// Repository cho bảng users.
/// Dùng để đăng nhập, lấy thông tin user theo email.
/// </summary>
public class UserRepository
{
    private readonly DbConnectionFactory _connFactory;

    public UserRepository(DbConnectionFactory connFactory) => _connFactory = connFactory;

    /// <summary>
    /// Tìm user theo email. Trả về null nếu không tìm thấy.
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
    /// Tạo user mới (đăng ký owner hoặc sale).
    /// Password được hash SHA256 trước khi lưu vào DB.
    /// Trả về UserRecord nếu thành công, null nếu thất bại.
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
            return null; // email đã tồn tại hoặc lỗi khác
        }
    }

    /// <summary>
    /// Kiểm tra đã có user nào trong hệ thống chưa.
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
    /// Tạo owner đầu tiên.
    /// </summary>
    public async Task<UserRecord?> CreateOwnerAsync(string email, string password)
        => await CreateAsync(email, password, "owner");
}

/// <summary>
/// DTO cho bảng users.
/// </summary>
public record UserRecord
{
    public int Id { get; init; }
    public string Email { get; init; } = string.Empty;
    /// <summary>SHA256 hash của password (Base64).</summary>
    public string Password { get; init; } = string.Empty;
    public string? Role { get; init; }
}
