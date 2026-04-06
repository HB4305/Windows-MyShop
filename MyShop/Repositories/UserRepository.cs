using MyShop.Models;
using MyShop.Services;
using Npgsql;

namespace MyShop.Repositories;

/// <summary>
/// Repository cho bảng users (email, password).
/// Dùng để đăng nhập.
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
            SELECT email, password
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
                Email = reader.GetString(0),
                Password = reader.GetString(1)
            };
        }

        return null;
    }

    /// <summary>
    /// Tạo user mới (đăng ký).
    /// Password được hash SHA256 base64 trước khi lưu vào DB.
    /// Trả về true nếu thành công.
    /// </summary>
    public async Task<bool> CreateAsync(string email, string password)
    {
        const string sql = @"
            INSERT INTO users (email, password)
            VALUES (@email, @password)";

        try
        {
            await using var conn = _connFactory.CreateConnection();
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("email", email.Trim().ToLowerInvariant());
            cmd.Parameters.AddWithValue("password", CredentialManager.ComputeHash(password));

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
        catch (NpgsqlException)
        {
            return false; // email đã tồn tại hoặc lỗi khác
        }
    }
}

/// <summary>
/// DTO cho bảng users.
/// </summary>
public record UserRecord
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
