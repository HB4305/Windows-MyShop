using MyShop.Models;
using MyShop.Services;
using Npgsql;

namespace MyShop.Repositories;

public class CategoryRepository
{
    private readonly DbConnectionFactory _connFactory;

    public CategoryRepository(DbConnectionFactory connFactory) => _connFactory = connFactory;

    public async Task<List<Category>> GetAllAsync()
    {
        const string sql = "SELECT id, name, description FROM categories ORDER BY name";
        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);

        var categories = new List<Category>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            categories.Add(new Category
            {
                Id = reader.GetInt32(0),
                Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2)
            });
        }
        return categories;
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        const string sql = "SELECT id, name, description FROM categories WHERE id = @id";
        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Category
            {
                Id = reader.GetInt32(0),
                Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2)
            };
        }
        return null;
    }

    public async Task<int> AddAsync(Category category)
    {
        const string sql = @"
            INSERT INTO categories (name, description)
            VALUES (@name, @description)
            RETURNING id";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("name", category.Name);
        cmd.Parameters.AddWithValue("description", (object?)category.Description ?? DBNull.Value);

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task UpdateAsync(Category category)
    {
        const string sql = @"
            UPDATE categories SET name = @name, description = @description
            WHERE id = @id";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", category.Id);
        cmd.Parameters.AddWithValue("name", category.Name);
        cmd.Parameters.AddWithValue("description", (object?)category.Description ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM categories WHERE id = @id";
        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<bool> HasProductsAsync(int categoryId)
    {
        const string sql = "SELECT EXISTS(SELECT 1 FROM sportitems WHERE category_id = @categoryId LIMIT 1)";
        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("categoryId", categoryId);
        var result = await cmd.ExecuteScalarAsync();
        return result is bool b && b;
    }
}
