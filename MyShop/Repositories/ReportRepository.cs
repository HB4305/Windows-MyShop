using MyShop.Models.ReportModels;
using MyShop.Services;
using Newtonsoft.Json;
using Npgsql;

namespace MyShop.Repositories;

public class ReportRepository
{
    private readonly DbConnectionFactory _connFactory;

    public ReportRepository(DbConnectionFactory connFactory) => _connFactory = connFactory;

    public async Task<ReportOverviewSnapshot> GetOverviewSnapshotAsync(
        DateTime startDate,
        DateTime endDate,
        string? categoryName = null,
        string? productName = null)
    {
        categoryName = NormalizeFilter(categoryName);
        productName = NormalizeFilter(productName);

        const string sql = @"
            SELECT * FROM get_report_overview(
                @p_start_date, @p_end_date, @p_category_name, @p_product_name)";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("p_start_date", startDate);
        cmd.Parameters.AddWithValue("p_end_date", endDate);
        cmd.Parameters.AddWithValue("p_category_name", (object?)categoryName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("p_product_name", (object?)productName ?? DBNull.Value);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new ReportOverviewSnapshot(
                reader.IsDBNull(0) ? 0m : reader.GetDecimal(0),
                reader.IsDBNull(1) ? 0 : reader.GetInt64(1),
                reader.IsDBNull(2) ? 0m : reader.GetDecimal(2),
                reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
            );
        }
        return new ReportOverviewSnapshot(0, 0, 0, 0);
    }

    public async Task<List<SoldQuantityData>> GetSoldQuantityDataAsync(
        DateTime startDate,
        DateTime endDate,
        string? categoryName,
        string? productName)
    {
        categoryName = NormalizeFilter(categoryName);
        productName = NormalizeFilter(productName);
        return await GetSoldQuantityDataByDayAsync(startDate, endDate, categoryName, productName);
    }

    public async Task<List<RevenueData>> GetRevenueDataAsync(DateTime startDate, DateTime endDate)
        => await GetRevenueByDayAsync(startDate, endDate);

    public async Task<List<ProfitByCategory>> GetProfitByCategoryAsync(DateTime startDate, DateTime endDate)
    {
        const string sql = @"
            SELECT * FROM get_category_profit(@p_start_date, @p_end_date)";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("p_start_date", startDate);
        cmd.Parameters.AddWithValue("p_end_date", endDate);

        var results = new List<ProfitByCategory>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new ProfitByCategory
            {
                CategoryName = reader.IsDBNull(0) ? "" : reader.GetString(0),
                Profit = reader.IsDBNull(1) ? 0m : reader.GetDecimal(1)
            });
        }
        return results;
    }

    public async Task<List<TopPerformingProduct>> GetTopPerformingProductsAsync(
        DateTime startDate,
        DateTime endDate,
        int limit = 5)
    {
        const string sql = @"
            SELECT * FROM get_top_performing_products(
                @p_start_date, @p_end_date, @p_category_name, @p_product_name, @p_limit)";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("p_start_date", startDate);
        cmd.Parameters.AddWithValue("p_end_date", endDate);
        cmd.Parameters.AddWithValue("p_category_name", DBNull.Value);
        cmd.Parameters.AddWithValue("p_product_name", DBNull.Value);
        cmd.Parameters.AddWithValue("p_limit", limit);

        var results = new List<TopPerformingProduct>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new TopPerformingProduct
            {
                // col 0=id, 1=product_name, 2=category_name, 3=image_urls, 4=total_quantity_sold, 5=gross_revenue, 6=profit
                ProductName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                GrossRevenue = reader.IsDBNull(5) ? 0m : reader.GetDecimal(5),
                Profit = reader.IsDBNull(6) ? 0m : reader.GetDecimal(6),
                TotalQuantitySold = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
            });
        }
        return results;
    }

    private async Task<List<SoldQuantityData>> GetSoldQuantityDataByDayAsync(
        DateTime startDate,
        DateTime endDate,
        string? categoryName,
        string? productName)
    {
        const string sql = @"
            SELECT * FROM get_product_sales_by_day(
                @p_start_date, @p_end_date, @p_category_name, @p_product_name)";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("p_start_date", startDate);
        cmd.Parameters.AddWithValue("p_end_date", endDate);
        cmd.Parameters.AddWithValue("p_category_name", (object?)categoryName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("p_product_name", (object?)productName ?? DBNull.Value);

        var results = new List<SoldQuantityData>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new SoldQuantityData
            {
                Date = reader.GetDateTime(0),
                QuantitySold = reader.IsDBNull(1) ? 0 : reader.GetInt64(1)
            });
        }
        return results;
    }

    private async Task<List<RevenueData>> GetRevenueByDayAsync(DateTime startDate, DateTime endDate)
    {
        const string sql = @"
            SELECT * FROM get_product_sales_by_day(
                @p_start_date, @p_end_date, @p_category_name, @p_product_name)";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("p_start_date", startDate);
        cmd.Parameters.AddWithValue("p_end_date", endDate);
        cmd.Parameters.AddWithValue("p_category_name", DBNull.Value);
        cmd.Parameters.AddWithValue("p_product_name", DBNull.Value);

        var results = new List<RevenueData>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new RevenueData
            {
                Date = reader.GetDateTime(0),
                GrossRevenue = reader.IsDBNull(2) ? 0m : reader.GetDecimal(2)
            });
        }
        return results;
    }

    private static string? NormalizeFilter(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    public readonly record struct ReportOverviewSnapshot(
        decimal Revenue,
        long QuantitySold,
        decimal Profit,
        int CustomersCount
    );
}
