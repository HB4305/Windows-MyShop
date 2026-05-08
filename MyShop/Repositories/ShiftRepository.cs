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
