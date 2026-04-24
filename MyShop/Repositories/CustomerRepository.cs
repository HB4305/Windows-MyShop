using MyShop.Models;
using MyShop.Services;
using Npgsql;

namespace MyShop.Repositories;

public class CustomerRepository
{
    private readonly DbConnectionFactory _connFactory;

    public CustomerRepository(DbConnectionFactory connFactory) => _connFactory = connFactory;

    private const string SelectColumns = "id, name, phone, address, created_at";

    public async Task<(List<Customer> Items, int TotalCount)> GetItemsAsync(int page, int pageSize, string keyword)
    {
        var items = new List<Customer>();
        int totalCount = 0;
        var offset = (page - 1) * pageSize;

        var where = string.IsNullOrWhiteSpace(keyword) 
            ? "" 
            : "WHERE name ILIKE @keyword OR phone ILIKE @keyword";

        // Query with analytical data (TotalSpent, OrderCount)
        var sql = $@"
            SELECT COUNT(*) OVER() as total_count, 
                   c.id, c.name, c.phone, c.address, c.created_at,
                   COALESCE(SUM(o.total_amount), 0) as total_spent,
                   COUNT(o.id) as order_count
            FROM customers c
            LEFT JOIN customerorders o ON c.id = o.customer_id
            {where}
            GROUP BY c.id
            ORDER BY c.name ASC
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
            items.Add(new Customer
            {
                Id = reader.GetInt32(1),
                Name = reader.GetString(2),
                Phone = reader.GetString(3),
                Address = reader.IsDBNull(4) ? null : reader.GetString(4),
                CreatedAt = reader.GetFieldValue<DateTimeOffset>(5),
                TotalSpent = reader.GetDecimal(6),
                OrderCount = (int)reader.GetInt64(7)
            });
        }

        return (items, totalCount);
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        const string sql = $@"
            SELECT {SelectColumns} 
            FROM customers 
            WHERE id = @id";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapFromReader(reader);
        }
        return null;
    }

    public async Task<Customer?> GetByPhoneAsync(string phone)
    {
        const string sql = $@"
            SELECT {SelectColumns} 
            FROM customers 
            WHERE phone = @phone";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("phone", phone);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapFromReader(reader);
        }
        return null;
    }

    public async Task<int> CreateAsync(Customer customer)
    {
        const string sql = @"
            INSERT INTO customers (name, phone, address)
            VALUES (@name, @phone, @address)
            RETURNING id";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        AddParams(cmd, customer);

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task UpdateAsync(Customer customer)
    {
        const string sql = @"
            UPDATE customers 
            SET name = @name, phone = @phone, address = @address
            WHERE id = @id";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", customer.Id);
        AddParams(cmd, customer);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM customers WHERE id = @id";
        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<CustomerOrder>> GetOrderHistoryAsync(int customerId)
    {
        const string sql = @"
            SELECT id, created_at, customer_name, customer_phone, 
                   shipping_address, order_type, status, payment_status, 
                   total_amount, notes, seller_id, seller_name
            FROM customerorders
            WHERE customer_id = @customerId
            ORDER BY created_at DESC";

        var orders = new List<CustomerOrder>();
        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("customerId", customerId);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            orders.Add(new CustomerOrder
            {
                Id = reader.GetInt32(0),
                CreatedAt = reader.IsDBNull(1) ? null : reader.GetFieldValue<DateTimeOffset>(1),
                CustomerName = reader.GetString(2),
                CustomerPhone = reader.GetString(3),
                ShippingAddress = reader.IsDBNull(4) ? null : reader.GetString(4),
                OrderType = reader.IsDBNull(5) ? null : reader.GetString(5),
                Status = reader.IsDBNull(6) ? null : reader.GetString(6),
                PaymentStatus = reader.IsDBNull(7) ? null : reader.GetString(7),
                TotalAmount = reader.IsDBNull(8) ? 0m : reader.GetDecimal(8),
                Notes = reader.IsDBNull(9) ? null : reader.GetString(9),
                SellerId = reader.IsDBNull(10) ? null : reader.GetInt32(10),
                SellerName = reader.IsDBNull(11) ? null : reader.GetString(11)
            });
        }
        return orders;
    }

    private static void AddParams(NpgsqlCommand cmd, Customer c)
    {
        cmd.Parameters.AddWithValue("name", c.Name);
        cmd.Parameters.AddWithValue("phone", c.Phone);
        cmd.Parameters.AddWithValue("address", (object?)c.Address ?? DBNull.Value);
    }

    private static Customer MapFromReader(NpgsqlDataReader r)
    {
        return new Customer
        {
            Id = r.GetInt32(0),
            Name = r.GetString(1),
            Phone = r.GetString(2),
            Address = r.IsDBNull(3) ? null : r.GetString(3),
            CreatedAt = r.GetFieldValue<DateTimeOffset>(4)
        };
    }
}
