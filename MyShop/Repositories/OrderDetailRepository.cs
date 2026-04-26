using MyShop.Models;
using MyShop.Services;
using Npgsql;

namespace MyShop.Repositories;

public class OrderDetailRepository
{
    private readonly DbConnectionFactory _connFactory;

    public OrderDetailRepository(DbConnectionFactory connFactory)
    {
        _connFactory = connFactory;
        EnsureDatabaseSchema();
    }

    private void EnsureDatabaseSchema()
    {
        try
        {
            using var conn = _connFactory.CreateConnection();
            conn.Open();
            using var cmd = new NpgsqlCommand(@"
                ALTER TABLE public.orderdetails ADD COLUMN IF NOT EXISTS variant_id bigint;
                ALTER TABLE public.orderdetails ADD COLUMN IF NOT EXISTS size text;
                ALTER TABLE public.orderdetails ADD COLUMN IF NOT EXISTS color text;
            ", conn);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[OrderDetailRepository] Failed to ensure schema: {ex.Message}");
        }
    }

    public async Task<List<OrderDetail>> GetAllAsync()
    {
        const string sql = @"
            SELECT id, order_id, item_id, item_name, quantity, unit_price, variant_id, size, color
            FROM orderdetails";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);

        var details = new List<OrderDetail>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            details.Add(ReadOrderDetail(reader));
        return details;
    }

    public async Task<List<OrderDetail>> GetByOrderIdAsync(int orderId)
    {
        const string sql = @"
            SELECT id, order_id, item_id, item_name, quantity, unit_price, variant_id, size, color
            FROM orderdetails
            WHERE order_id = @orderId";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("orderId", orderId);

        var details = new List<OrderDetail>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            details.Add(ReadOrderDetail(reader));
        return details;
    }

    public async Task<OrderDetail> CreateAsync(OrderDetail detail)
    {
        const string sql = @"
            INSERT INTO orderdetails (order_id, item_id, item_name, quantity, unit_price, variant_id, size, color)
            VALUES (@orderId, @itemId, @itemName, @quantity, @unitPrice, @variantId, @size, @color)
            RETURNING id";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        AddDetailParams(cmd, detail);

        var result = await cmd.ExecuteScalarAsync();
        detail.Id = Convert.ToInt32(result);
        return detail;
    }

    public async Task CreateBulkAsync(List<OrderDetail> details)
    {
        if (details.Count == 0) return;

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var writer = await conn.BeginBinaryImportAsync(
            "COPY orderdetails (order_id, item_id, item_name, quantity, unit_price, variant_id, size, color) FROM STDIN (FORMAT BINARY)");

        foreach (var detail in details)
        {
            await writer.StartRowAsync();
            await writer.WriteAsync((object?)detail.OrderId ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
            await writer.WriteAsync((object?)detail.ItemId ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
            await writer.WriteAsync((object?)detail.ItemName ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Varchar);
            await writer.WriteAsync(detail.Quantity, NpgsqlTypes.NpgsqlDbType.Integer);
            await writer.WriteAsync(detail.UnitPrice, NpgsqlTypes.NpgsqlDbType.Numeric);
            await writer.WriteAsync((object?)detail.VariantId ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Bigint);
            await writer.WriteAsync((object?)detail.Size ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Varchar);
            await writer.WriteAsync((object?)detail.Color ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Varchar);
        }

        await writer.CompleteAsync();
    }

    public async Task<OrderDetail> UpdateAsync(OrderDetail detail)
    {
        const string sql = @"
            UPDATE orderdetails SET
                order_id = @orderId, item_id = @itemId,
                item_name = @itemName, quantity = @quantity, unit_price = @unitPrice,
                variant_id = @variantId, size = @size, color = @color
            WHERE id = @id";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", detail.Id);
        AddDetailParams(cmd, detail);
        await cmd.ExecuteNonQueryAsync();
        return detail;
    }

    public async Task DeleteByOrderIdAsync(int orderId)
    {
        const string sql = "DELETE FROM orderdetails WHERE order_id = @orderId";
        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("orderId", orderId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM orderdetails WHERE id = @id";
        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    private static void AddDetailParams(NpgsqlCommand cmd, OrderDetail d)
    {
        cmd.Parameters.AddWithValue("orderId", (object?)d.OrderId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("itemId", (object?)d.ItemId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("itemName", (object?)d.ItemName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("quantity", d.Quantity);
        cmd.Parameters.AddWithValue("unitPrice", d.UnitPrice);
        cmd.Parameters.AddWithValue("variantId", (object?)d.VariantId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("size", (object?)d.Size ?? DBNull.Value);
        cmd.Parameters.AddWithValue("color", (object?)d.Color ?? DBNull.Value);
    }

    private static OrderDetail ReadOrderDetail(NpgsqlDataReader r)
    {
        return new OrderDetail
        {
            Id = r.GetInt32(0),
            OrderId = r.IsDBNull(1) ? null : r.GetInt32(1),
            ItemId = r.IsDBNull(2) ? null : r.GetInt32(2),
            ItemName = r.IsDBNull(3) ? null : r.GetString(3),
            Quantity = r.IsDBNull(4) ? 0 : r.GetInt32(4),
            UnitPrice = r.IsDBNull(5) ? 0m : r.GetDecimal(5),
            VariantId = r.IsDBNull(6) ? null : r.GetInt64(6),
            Size = r.IsDBNull(7) ? null : r.GetString(7),
            Color = r.IsDBNull(8) ? null : r.GetString(8)
        };
    }
}
