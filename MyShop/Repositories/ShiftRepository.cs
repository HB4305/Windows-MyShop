using MyShop.Models;
using MyShop.Services;
using Npgsql;

namespace MyShop.Repositories;

public class ShiftRepository
{
    private readonly DbConnectionFactory _connFactory;

    public ShiftRepository(DbConnectionFactory connFactory) => _connFactory = connFactory;

    public async Task<Shift?> GetByIdAsync(int shiftId)
    {
        const string sql = @"
            SELECT id, user_id, start_time, end_time, starting_cash, actual_cash_total, notes, status
            FROM shifts
            WHERE id = @shiftId";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("shiftId", shiftId);

        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadShift(reader) : null;
    }

    public async Task<Shift?> GetActiveByUserAsync(int userId)
    {
        const string sql = @"
            SELECT id, user_id, start_time, end_time, starting_cash, actual_cash_total, notes, status
            FROM shifts
            WHERE user_id = @userId
              AND status = 'Open'
            ORDER BY start_time DESC
            LIMIT 1";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("userId", userId);

        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadShift(reader) : null;
    }

    public async Task<Shift> OpenShiftAsync(int userId, decimal startingCash)
    {
        const string sql = @"
            INSERT INTO shifts (user_id, starting_cash, status)
            VALUES (@userId, @startingCash, 'Open')
            RETURNING id, user_id, start_time, end_time, starting_cash, actual_cash_total, notes, status";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("userId", userId);
        cmd.Parameters.AddWithValue("startingCash", startingCash);

        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();
        return ReadShift(reader);
    }

    public async Task<decimal> GetExpectedCashAsync(int shiftId)
    {
        const string sql = @"
            SELECT COALESCE(SUM(total_amount), 0)
            FROM customerorders
            WHERE shift_id = @shiftId
              AND payment_method = 'Cash'";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("shiftId", shiftId);

        var scalar = await cmd.ExecuteScalarAsync();
        return Convert.ToDecimal(scalar);
    }

    public async Task<ShiftMetrics> GetMetricsAsync(int shiftId)
    {
        const string sql = @"
            SELECT
                COALESCE(SUM(CASE WHEN COALESCE(status, 'Pending') <> 'Cancelled' THEN COALESCE(total_amount, 0) ELSE 0 END), 0) AS total_revenue,
                COUNT(*) FILTER (WHERE COALESCE(status, 'Pending') <> 'Cancelled') AS customer_count
            FROM customerorders
            WHERE shift_id = @shiftId";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("shiftId", shiftId);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return new ShiftMetrics();
        }

        return new ShiftMetrics
        {
            TotalRevenue = reader.IsDBNull(0) ? 0m : reader.GetDecimal(0),
            CustomerCount = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetInt64(1))
        };
    }

    public async Task<List<ShiftReportLogEntry>> GetClosedReportLogsAsync(int? userId = null)
    {
        var sql = @"
            SELECT
                s.id,
                s.user_id,
                COALESCE(u.email, '') AS staff_email,
                s.start_time,
                s.end_time,
                COALESCE(s.starting_cash, 0) AS starting_cash,
                COALESCE(s.actual_cash_total, 0) AS actual_cash_total,
                COALESCE(s.notes, '') AS notes,
                COALESCE(SUM(CASE WHEN COALESCE(co.status, 'Pending') <> 'Cancelled' THEN COALESCE(co.total_amount, 0) ELSE 0 END), 0) AS total_revenue,
                COUNT(*) FILTER (WHERE co.id IS NOT NULL AND COALESCE(co.status, 'Pending') <> 'Cancelled') AS customer_count,
                COALESCE(SUM(CASE WHEN COALESCE(co.payment_method, '') = 'Cash' AND COALESCE(co.status, 'Pending') <> 'Cancelled' THEN COALESCE(co.total_amount, 0) ELSE 0 END), 0) AS expected_cash
            FROM shifts s
            INNER JOIN users u ON u.id = s.user_id
            LEFT JOIN customerorders co ON co.shift_id = s.id
            WHERE s.status = 'Closed'";

        if (userId.HasValue)
        {
            sql += " AND s.user_id = @userId";
        }

        sql += @"
            GROUP BY s.id, s.user_id, u.email, s.start_time, s.end_time, s.starting_cash, s.actual_cash_total, s.notes
            ORDER BY s.end_time DESC, s.id DESC";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        if (userId.HasValue)
        {
            cmd.Parameters.AddWithValue("userId", userId.Value);
        }

        var items = new List<ShiftReportLogEntry>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new ShiftReportLogEntry
            {
                ShiftId = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                StaffEmail = reader.GetString(2),
                StartTime = reader.IsDBNull(3) ? null : reader.GetFieldValue<DateTimeOffset>(3),
                EndTime = reader.IsDBNull(4) ? null : reader.GetFieldValue<DateTimeOffset>(4),
                StartingCash = reader.IsDBNull(5) ? 0m : reader.GetDecimal(5),
                ActualCashTotal = reader.IsDBNull(6) ? 0m : reader.GetDecimal(6),
                Notes = reader.IsDBNull(7) ? null : reader.GetString(7),
                TotalRevenue = reader.IsDBNull(8) ? 0m : reader.GetDecimal(8),
                CustomerCount = reader.IsDBNull(9) ? 0 : Convert.ToInt32(reader.GetInt64(9)),
                ExpectedCash = reader.IsDBNull(10) ? 0m : reader.GetDecimal(10)
            });
        }

        return items;
    }

    public async Task<Shift> CloseShiftAsync(int shiftId, decimal actualCashTotal, string? notes)
    {
        const string sql = @"
            UPDATE shifts
            SET end_time = now(),
                actual_cash_total = @actualCashTotal,
                notes = @notes,
                status = 'Closed'
            WHERE id = @shiftId
              AND status = 'Open'
            RETURNING id, user_id, start_time, end_time, starting_cash, actual_cash_total, notes, status";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("shiftId", shiftId);
        cmd.Parameters.AddWithValue("actualCashTotal", actualCashTotal);
        cmd.Parameters.AddWithValue("notes", (object?)notes ?? DBNull.Value);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            throw new InvalidOperationException("Shift is already closed or does not exist.");
        }

        return ReadShift(reader);
    }

    private static Shift ReadShift(NpgsqlDataReader reader)
    {
        return new Shift
        {
            Id = reader.GetInt32(0),
            UserId = reader.GetInt32(1),
            StartTime = reader.IsDBNull(2) ? null : reader.GetFieldValue<DateTimeOffset>(2),
            EndTime = reader.IsDBNull(3) ? null : reader.GetFieldValue<DateTimeOffset>(3),
            StartingCash = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4),
            ActualCashTotal = reader.IsDBNull(5) ? 0m : reader.GetDecimal(5),
            Notes = reader.IsDBNull(6) ? null : reader.GetString(6),
            Status = reader.IsDBNull(7) ? "Open" : reader.GetString(7)
        };
    }
}
