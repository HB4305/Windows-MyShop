namespace MyShop.Models.DashboardModels;

public class DashboardRecentOrder
{
  public int Id { get; set; }
  public DateTime? CreatedAt { get; set; }
  public string CustomerName { get; set; } = string.Empty;
  public string? Status { get; set; }
  public decimal? TotalPrice { get; set; }
  public List<DashboardRecentOrderDetail> Details { get; set; } = [];

  public class DashboardRecentOrderDetail
  {
    public int Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
  }
}
