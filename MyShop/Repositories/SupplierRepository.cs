using MyShop.Models;
using MyShop.Services;
using Npgsql;

namespace MyShop.Repositories;

public class SupplierRepository
{
    private readonly DbConnectionFactory _connFactory;

    public SupplierRepository(DbConnectionFactory connFactory) => _connFactory = connFactory;

    private const string SelectColumns = "id, name, contact_phone, supplier_type";

    public async Task<(List<Supplier> Items, int TotalCount)> GetItemsAsync(int page, int pageSize, string keyword)
    {
        var items = new List<Supplier>();
        int totalCount = 0;
        var offset = (page - 1) * pageSize;

        var where = string.IsNullOrWhiteSpace(keyword)
            ? ""
            : "WHERE name ILIKE @keyword OR contact_phone ILIKE @keyword";

        var sql = $@"
            SELECT COUNT(*) OVER() as total_count, 
                   {SelectColumns}
            FROM suppliers
            {where}
            ORDER BY name ASC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("offset", offset);
        cmd.Parameters.AddWithValue("pageSize", pageSize);
        if (!string.IsNullOrWhiteSpace(keyword))
            cmd.Parameters.AddWithValue("keyword", $"%{keyword}%");

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            totalCount = reader.GetInt32(0);
            items.Add(new Supplier
            {
                Id = reader.GetInt32(1),
                Name = reader.GetString(2),
                ContactPhone = reader.IsDBNull(3) ? null : reader.GetString(3),
                SupplierType = reader.IsDBNull(4) ? null : reader.GetString(4)
            });
        }

        return (items, totalCount);
    }

    public async Task<List<Supplier>> GetAllAsync()
    {
        var sql = $"SELECT {SelectColumns} FROM suppliers ORDER BY name ASC";
        var items = new List<Supplier>();

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new Supplier
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                ContactPhone = reader.IsDBNull(2) ? null : reader.GetString(2),
                SupplierType = reader.IsDBNull(3) ? null : reader.GetString(3)
            });
        }
        return items;
    }

    public async Task<int> CreateAsync(Supplier supplier)
    {
        const string sql = @"
            INSERT INTO suppliers (name, contact_phone, supplier_type)
            VALUES (@name, @contact_phone, @supplier_type)
            RETURNING id";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        AddParams(cmd, supplier);

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task UpdateAsync(Supplier supplier)
    {
        const string sql = @"
            UPDATE suppliers 
            SET name = @name, contact_phone = @contact_phone, supplier_type = @supplier_type
            WHERE id = @id";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", supplier.Id);
        AddParams(cmd, supplier);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM suppliers WHERE id = @id";
        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    private static void AddParams(NpgsqlCommand cmd, Supplier s)
    {
        cmd.Parameters.AddWithValue("name", s.Name);
        cmd.Parameters.AddWithValue("contact_phone", (object?)s.ContactPhone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("supplier_type", (object?)s.SupplierType ?? DBNull.Value);
    }
}
