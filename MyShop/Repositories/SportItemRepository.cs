using MyShop.Models;
using MyShop.Models.DashboardModels;

namespace MyShop.Repositories;

public class SportItemRepository
{
  private readonly Supabase.Client _client;

  public SportItemRepository(Supabase.Client client) => _client = client;

  // Đếm tổng sản phẩm
  public async Task<int> GetTotalCountAsync()
    => await _client.From<SportItem>().Count(Postgrest.Constants.CountType.Exact);

  public async Task<List<DashboardLowStockProduct>> GetLowStockProductsAsync(int threshold = 5, int limit = 5)
  {
    var response = await _client
      .From<SportItem>()
      .Where(item => item.StockQuantity < threshold)
      .Order("stock_quantity", Postgrest.Constants.Ordering.Ascending)
      .Limit(limit)
      .Get();

    return response.Models
      .Select(item => new DashboardLowStockProduct
      {
        ItemId = item.Id,
        Name = item.Name,
        StockQuantity = item.StockQuantity ?? 0,
        ImageUrls = item.ImageUrls
      })
      .ToList();
  }


}
