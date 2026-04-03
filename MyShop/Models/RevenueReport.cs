namespace MyShop.Models;

public class RevenueReport
{
    public DateTime Date { get; set; }
    public int TotalOrders { get; set; }
    public decimal? GrossRevenue { get; set; }
}
