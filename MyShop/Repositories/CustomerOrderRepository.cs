using MyShop.Models;
using MyShop.Services;
using Npgsql;

namespace MyShop.Repositories;

public class CustomerOrderRepository
{
    private readonly DbConnectionFactory _connFactory;

    public CustomerOrderRepository(DbConnectionFactory connFactory) => _connFactory = connFactory;

    // ═══════════════════════════════════════════════════════════════════════
    // Read
    // ═══════════════════════════════════════════════════════════════════════

    private const string SelectColumns =
        @"id, created_at, customer_name, customer_phone,
          shipping_address, order_type, status, payment_status,
          total_amount, notes, COALESCE(seller_id, 0), COALESCE(seller_name, '')";

    /// <summary>
    /// Gets all orders (owner only).
    /// </summary>
    public async Task<List<CustomerOrder>> GetAllAsync()
    {
        string sql = $"SELECT {SelectColumns} FROM customerorders ORDER BY created_at DESC";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);

        var orders = new List<CustomerOrder>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            orders.Add(ReadCustomerOrder(reader));
        return orders;
    }

    /// <summary>
    /// Gets orders for a specific sale (sale role only).
    /// </summary>
    public async Task<List<CustomerOrder>> GetBySellerIdAsync(int sellerId)
    {
        string sql = $@"
            SELECT {SelectColumns}
            FROM customerorders
            WHERE seller_id = @sellerId
            ORDER BY created_at DESC";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("sellerId", sellerId);

        var orders = new List<CustomerOrder>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            orders.Add(ReadCustomerOrder(reader));
        return orders;
    }

    public async Task<CustomerOrder?> GetByIdAsync(int id)
    {
        string sql = $"SELECT {SelectColumns} FROM customerorders WHERE id = @id";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return ReadCustomerOrder(reader);
        return null;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Create / Update / Delete
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new order. Automatically assigns seller_id and seller_name.
    /// </summary>
    public async Task<CustomerOrder> CreateAsync(CustomerOrder order, int sellerId, string sellerName)
    {
        const string sql = @"
            INSERT INTO customerorders
                (customer_name, customer_phone, shipping_address,
                 order_type, status, payment_status, total_amount, notes,
                 seller_id, seller_name)
            VALUES (@customerName, @customerPhone, @shippingAddress,
                    @orderType, @status, @paymentStatus, @totalAmount, @notes,
                    @sellerId, @sellerName)
            RETURNING id, created_at";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        AddOrderParams(cmd, order);
        cmd.Parameters.AddWithValue("sellerId", sellerId);
        cmd.Parameters.AddWithValue("sellerName", sellerName ?? "");

        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();
        order.Id = reader.GetInt32(0);
        order.CreatedAt = reader.GetFieldValue<DateTimeOffset>(1);
        order.SellerId = sellerId;
        order.SellerName = sellerName;
        return order;
    }

    public async Task<CustomerOrder> UpdateAsync(CustomerOrder order)
    {
        const string sql = @"
            UPDATE customerorders SET
                customer_name = @customerName,
                customer_phone = @customerPhone,
                shipping_address = @shippingAddress,
                order_type = @orderType,
                status = @status,
                payment_status = @paymentStatus,
                total_amount = @totalAmount,
                notes = @notes
            WHERE id = @id";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", order.Id);
        AddOrderParams(cmd, order);
        await cmd.ExecuteNonQueryAsync();
        return order;
    }

    public async Task UpdateStatusAsync(int id, string status)
    {
        const string sql = "UPDATE customerorders SET status = @status WHERE id = @id";
        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("status", status);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdatePaymentStatusAsync(int id, string paymentStatus)
    {
        const string sql = "UPDATE customerorders SET payment_status = @paymentStatus WHERE id = @id";
        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("paymentStatus", paymentStatus);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM customerorders WHERE id = @id";
        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════════════════════════════════

    private static void AddOrderParams(NpgsqlCommand cmd, CustomerOrder o)
    {
        cmd.Parameters.AddWithValue("customerName", o.CustomerName);
        cmd.Parameters.AddWithValue("customerPhone", o.CustomerPhone);
        cmd.Parameters.AddWithValue("shippingAddress", (object?)o.ShippingAddress ?? DBNull.Value);
        cmd.Parameters.AddWithValue("orderType", (object?)o.OrderType ?? DBNull.Value);
        cmd.Parameters.AddWithValue("status", o.Status ?? "Pending");
        cmd.Parameters.AddWithValue("paymentStatus", o.PaymentStatus ?? "Unpaid");
        cmd.Parameters.AddWithValue("totalAmount", (object?)o.TotalAmount ?? 0m);
        cmd.Parameters.AddWithValue("notes", (object?)o.Notes ?? DBNull.Value);
    }

    private static CustomerOrder ReadCustomerOrder(NpgsqlDataReader r)
    {
        return new CustomerOrder
        {
            Id = r.GetInt32(0),
            CreatedAt = r.IsDBNull(1) ? null : r.GetFieldValue<DateTimeOffset>(1),
            CustomerName = r.IsDBNull(2) ? "" : r.GetString(2),
            CustomerPhone = r.IsDBNull(3) ? "" : r.GetString(3),
            ShippingAddress = r.IsDBNull(4) ? null : r.GetString(4),
            OrderType = r.IsDBNull(5) ? null : r.GetString(5),
            Status = r.IsDBNull(6) ? null : r.GetString(6),
            PaymentStatus = r.IsDBNull(7) ? null : r.GetString(7),
            TotalAmount = r.IsDBNull(8) ? 0m : r.GetDecimal(8),
            Notes = r.IsDBNull(9) ? null : r.GetString(9),
            SellerId = r.IsDBNull(10) ? null : r.GetInt32(10),
            SellerName = r.IsDBNull(11) ? null : r.GetString(11)
        };
    }
}
