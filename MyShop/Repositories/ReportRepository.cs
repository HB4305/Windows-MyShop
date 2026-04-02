using MyShop.Models.ReportModels;
using Newtonsoft.Json;

namespace MyShop.Repositories;

public class ReportRepository
{
  private readonly Supabase.Client _client;

  public ReportRepository(Supabase.Client client) => _client = client;

  public async Task<ReportOverviewSnapshot> GetOverviewSnapshotAsync(
    DateTime startDate,
    DateTime endDate,
    string? categoryName = null,
    string? productName = null)
  {
    categoryName = NormalizeFilter(categoryName);
    productName = NormalizeFilter(productName);

    var response = await _client.Rpc<List<ReportOverviewRow>>(
      "get_report_overview",
      new
      {
        p_start_date = startDate,
        p_end_date = endDate,
        p_category_name = categoryName,
        p_product_name = productName
      }
    );

    var row = response?.FirstOrDefault() ?? new ReportOverviewRow();
    return new ReportOverviewSnapshot(
      row.TotalRevenue,
      row.TotalQuantitySold,
      row.TotalProfit,
      row.TotalCustomers
    );
  }

  public async Task<List<SoldQuantityData>> GetSoldQuantityDataAsync(
    DateTime startDate,
    DateTime endDate,
    string? categoryName,
    string? productName)
  {
    categoryName = NormalizeFilter(categoryName);
    productName = NormalizeFilter(productName);

    return await GetSoldQuantityDataByDayAsync(startDate, endDate, categoryName, productName);
  }

  public Task<List<RevenueData>> GetRevenueDataAsync(DateTime startDate, DateTime endDate)
    => GetRevenueByDayAsync(startDate, endDate);

  public async Task<List<ProfitByCategory>> GetProfitByCategoryAsync(DateTime startDate, DateTime endDate)
  {
    var response = await _client.Rpc<List<ProfitByCategory>>(
      "get_category_profit",
      new
      {
        p_start_date = startDate,
        p_end_date = endDate
      }
    );

    return response ?? [];
  }

  public async Task<List<TopPerformingProduct>> GetTopPerformingProductsAsync(
    DateTime startDate,
    DateTime endDate,
    int limit = 5)
  {
    var response = await _client.Rpc<List<TopPerformingProduct>>(
      "get_top_performing_products",
      new
      {
        p_start_date = startDate,
        p_end_date = endDate,
        p_category_name = (string?)null,
        p_product_name = (string?)null,
        p_limit = limit
      }
    );

    return response ?? [];
  }

  private async Task<List<SoldQuantityData>> GetSoldQuantityDataByDayAsync(
    DateTime startDate,
    DateTime endDate,
    string? categoryName,
    string? productName)
  {
    var response = await _client.Rpc<List<ProductSalesByDayRow>>(
      "get_product_sales_by_day",
      new
      {
        p_start_date = startDate,
        p_end_date = endDate,
        p_category_name = categoryName,
        p_product_name = productName
      }
    );

    return response?.Select(row => new SoldQuantityData
    {
      Date = row.Day,
      QuantitySold = row.QuantitySold
    }).ToList() ?? [];
  }

  private async Task<List<RevenueData>> GetRevenueByDayAsync(DateTime startDate, DateTime endDate)
  {
    var response = await _client.Rpc<List<ProductSalesByDayRow>>(
      "get_product_sales_by_day",
      new
      {
        p_start_date = startDate,
        p_end_date = endDate,
        p_category_name = (string?)null,
        p_product_name = (string?)null
      }
    );

    return response?.Select(row => new RevenueData
    {
      Date = row.Day,
      GrossRevenue = row.GrossRevenue
    }).ToList() ?? [];
  }

  private static string? NormalizeFilter(string? value)
  {
    var trimmed = value?.Trim();
    return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
  }

  public readonly record struct ReportOverviewSnapshot(
    decimal Revenue,
    long QuantitySold,
    decimal Profit,
    int CustomersCount
  );

  private sealed class ReportOverviewRow
  {
    [JsonProperty("total_revenue")]
    public decimal TotalRevenue { get; set; }

    [JsonProperty("total_quantity_sold")]
    public long TotalQuantitySold { get; set; }

    [JsonProperty("total_profit")]
    public decimal TotalProfit { get; set; }

    [JsonProperty("total_customers")]
    public int TotalCustomers { get; set; }
  }

  private sealed class ProductSalesByDayRow
  {
    [JsonProperty("day")]
    public DateTime Day { get; set; }

    [JsonProperty("quantity_sold")]
    public long QuantitySold { get; set; }

    [JsonProperty("gross_revenue")]
    public decimal GrossRevenue { get; set; }
  }

}
