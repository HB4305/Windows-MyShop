using MyShop.Models;
using MyShop.Repositories;

namespace MyShop.Services;

public class ShiftService
{
    private readonly ShiftRepository _repository;

    public ShiftService(ShiftRepository repository) => _repository = repository;

    public Task<Shift?> GetActiveShiftAsync(int userId)
        => _repository.GetActiveByUserAsync(userId);

    public Task<decimal> GetExpectedCashAsync(int shiftId)
        => _repository.GetExpectedCashAsync(shiftId);

    public Task<ShiftMetrics> GetMetricsAsync(int shiftId)
        => _repository.GetMetricsAsync(shiftId);

    public Task<List<ShiftReportLogEntry>> GetClosedReportLogsAsync(int? userId = null)
        => _repository.GetClosedReportLogsAsync(userId);

    public async Task<Shift> OpenShiftAsync(int userId, decimal startingCash)
    {
        if (startingCash < 0)
        {
            throw new InvalidOperationException("Starting cash cannot be negative.");
        }

        var activeShift = await _repository.GetActiveByUserAsync(userId);
        if (activeShift is not null)
        {
            throw new InvalidOperationException("An active shift is already open for this user.");
        }

        return await _repository.OpenShiftAsync(userId, startingCash);
    }

    public async Task<(Shift ClosedShift, decimal ExpectedCash)> CloseShiftAsync(int shiftId, decimal actualCashTotal, string? notes)
    {
        if (actualCashTotal < 0)
        {
            throw new InvalidOperationException("Actual cash cannot be negative.");
        }

        var expectedCash = await _repository.GetExpectedCashAsync(shiftId);
        var closedShift = await _repository.CloseShiftAsync(shiftId, actualCashTotal, notes);
        return (closedShift, expectedCash);
    }
}
