namespace MyShop.Models;

public class ShiftMetrics
{
    public decimal TotalRevenue { get; set; }

    public int CustomerCount { get; set; }

    public decimal AverageSpendPerCustomer => CustomerCount <= 0
        ? 0m
        : Math.Round(TotalRevenue / CustomerCount, 2);
}
