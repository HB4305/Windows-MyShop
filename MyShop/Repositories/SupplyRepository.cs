using MyShop.Models;
using MyShop.Services;
using MyShop.Utils;
using Npgsql;

namespace MyShop.Repositories;

public class SupplyRepository
{
    private readonly DbConnectionFactory _connFactory;

    public SupplyRepository(DbConnectionFactory connFactory) => _connFactory = connFactory;

    public async Task<int> GetSuppliedProductCountByDateAsync(DateTime date)
    {
        var (start, end) = DateTimeUtils.GetDayRange(date);
        const string sql = @"
            SELECT COUNT(DISTINCT sd.item_id)
            FROM supplyorders so
            JOIN supplydetails sd ON sd.supply_id = so.id
            WHERE so.import_date >= @start
              AND so.import_date < @end
              AND sd.item_id IS NOT NULL";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("start", start);
        cmd.Parameters.AddWithValue("end", end);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> GetSuppliedProductCountByMonthAsync(DateTime referenceTime)
    {
        var prevMonth = referenceTime.AddMonths(-1);
        var start = new DateTime(prevMonth.Year, prevMonth.Month, 1);
        var end = start.AddMonths(1);

        const string sql = @"
            SELECT COUNT(DISTINCT sd.item_id)
            FROM supplyorders so
            JOIN supplydetails sd ON sd.supply_id = so.id
            WHERE so.import_date >= @start
              AND so.import_date < @end
              AND sd.item_id IS NOT NULL";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("start", start);
        cmd.Parameters.AddWithValue("end", end);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
}
