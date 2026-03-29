using System.Linq;

namespace MyShop.Models.DashboardModels;

public class DashboardLowStockProduct
{
  public int ItemId { get; set; }
  public string Name { get; set; } = string.Empty;
  public int StockQuantity { get; set; }
  public string[] ImageUrls { get; set; } = [];
  public string? ImageUrl => ImageUrls.FirstOrDefault();
  public string ImageUrlsText => string.Join(", ", ImageUrls);
}
