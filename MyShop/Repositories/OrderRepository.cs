using MyShop.Models;
using MyShop.Models.DashboardModels;
using MyShop.Services;
using MyShop.Utils;
using Npgsql;

namespace MyShop.Repositories;

public class OrderRepository
{
    private readonly DbConnectionFactory _connFactory;

    public OrderRepository(DbConnectionFactory connFactory) => _connFactory = connFactory;

    public async Task<int> GetDateOrderCountAsync(DateTime date)
    {
        var (start, end) = DateTimeUtils.GetDayRange(date);
        const string sql = @"
            SELECT COUNT(*) FROM customerorders
            WHERE created_at >= @start AND created_at < @end";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("start", start);
        cmd.Parameters.AddWithValue("end", end);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> GetAvgOrdersAsync(DateTime start, DateTime end)
    {
        const string sql = @"
            SELECT COUNT(*) FROM customerorders
            WHERE created_at >= @start AND created_at < @end";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("start", start);
        cmd.Parameters.AddWithValue("end", end);
        var result = await cmd.ExecuteScalarAsync();
        var totalOrders = Convert.ToInt32(result);
        var totalDays = Math.Max(1, (end.Date - start.Date).TotalDays);
        return (int)Math.Round(totalOrders / totalDays, MidpointRounding.AwayFromZero);
    }

    public async Task<decimal> GetDateRevenueAsync(DateTime date)
    {
        var (start, end) = DateTimeUtils.GetDayRange(date);
        const string sql = @"
            SELECT COALESCE(SUM(total_amount), 0) FROM customerorders
            WHERE status = 'Delivered'
              AND created_at >= @start AND created_at < @end";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("start", start);
        cmd.Parameters.AddWithValue("end", end);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToDecimal(result);
    }

    public async Task<List<DashboardRecentOrder>> GetRecentOrdersAsync(int limit = 3)
    {
        // Load orders
        var ordersSql = $@"
            SELECT id, created_at, customer_name, status, total_amount
            FROM customerorders
            ORDER BY created_at DESC
            LIMIT @limit";

        // Load all details
        const string detailsSql = "SELECT id, order_id, item_id, item_name, quantity, unit_price FROM orderdetails";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();

        // Load details first
        var detailsMap = new Dictionary<int, List<DashboardRecentOrder.DashboardRecentOrderDetail>>();
        await using (var cmd = new NpgsqlCommand(detailsSql, conn))
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var orderId = reader.GetInt32(1);
                if (!detailsMap.TryGetValue(orderId, out var list))
                {
                    list = new List<DashboardRecentOrder.DashboardRecentOrderDetail>();
                    detailsMap[orderId] = list;
                }
                list.Add(new DashboardRecentOrder.DashboardRecentOrderDetail
                {
                    Id = reader.GetInt32(0),
                    Quantity = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                    UnitPrice = reader.IsDBNull(5) ? 0m : reader.GetDecimal(5)
                });
            }
        }

        // Load orders
        var orders = new List<DashboardRecentOrder>();
        await using (var cmd = new NpgsqlCommand(ordersSql, conn))
        {
            cmd.Parameters.AddWithValue("limit", limit);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var id = reader.GetInt32(0);
                orders.Add(new DashboardRecentOrder
                {
                    Id = id,
                    CreatedAt = reader.IsDBNull(1) ? null : reader.GetFieldValue<DateTimeOffset>(1),
                    CustomerName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Status = reader.IsDBNull(3) ? null : reader.GetString(3),
                    TotalPrice = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4),
                    Details = detailsMap.GetValueOrDefault(id, new List<DashboardRecentOrder.DashboardRecentOrderDetail>())
                });
            }
        }

        return orders;
    }

    public async Task<List<RevenueReport>> GetRevenuePointsAsync(DateTime start, DateTime end)
    {
        const string sql = @"
            SELECT date, total_orders, gross_revenue
            FROM view_revenue_report
            WHERE date >= @start AND date < @end
            ORDER BY date ASC";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("start", start);
        cmd.Parameters.AddWithValue("end", end);

        var reports = new List<RevenueReport>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            reports.Add(new RevenueReport
            {
                Date = reader.GetDateTime(0),
                TotalOrders = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                GrossRevenue = reader.IsDBNull(2) ? null : reader.GetDecimal(2)
            });
        }
        return reports;
    }

    public async Task<List<DashboardTopSellerProduct>> GetTopSellingProductsAsync(
        DateTime currentStart, DateTime currentEnd,
        DateTime prevStart, DateTime prevEnd,
        int limit = 5)
    {
        const string sql = @"
            SELECT * FROM get_top_selling_products(
                @p_start, @p_end, @p_prev_start, @p_prev_end, @p_limit)";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("p_start", currentStart);
        cmd.Parameters.AddWithValue("p_end", currentEnd);
        cmd.Parameters.AddWithValue("p_prev_start", prevStart);
        cmd.Parameters.AddWithValue("p_prev_end", prevEnd);
        cmd.Parameters.AddWithValue("p_limit", limit);

        var results = new List<DashboardTopSellerProduct>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new DashboardTopSellerProduct
            {
                // col 0=item_id, 1=name, 2=category_name, 3=selling_price, 4=image_urls
                Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                QuantitySold = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                CurrPeriodRevenue = reader.IsDBNull(6) ? 0m : reader.GetDecimal(6),
                PrevPeriodRevenue = reader.IsDBNull(7) ? 0m : reader.GetDecimal(7)
            });
        }
        return results;
    }
}
