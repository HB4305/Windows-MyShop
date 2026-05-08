namespace MyShop.Models;

public class ShiftReportLogEntry
{
    public int ShiftId { get; set; }

    public int UserId { get; set; }

    public string StaffEmail { get; set; } = string.Empty;

    public DateTimeOffset? StartTime { get; set; }

    public DateTimeOffset? EndTime { get; set; }

    public decimal StartingCash { get; set; }

    public decimal ExpectedCash { get; set; }

    public decimal ActualCashTotal { get; set; }

    public decimal TotalRevenue { get; set; }

    public int CustomerCount { get; set; }

    public string? Notes { get; set; }

    public decimal Discrepancy => ActualCashTotal - ExpectedCash;

    public decimal AverageSpendPerCustomer => CustomerCount <= 0
        ? 0m
        : Math.Round(TotalRevenue / CustomerCount, 2);
}
