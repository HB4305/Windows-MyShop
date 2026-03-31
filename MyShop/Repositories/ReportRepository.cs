using MyShop.Models.ReportModels;

namespace MyShop.Repositories;

public class ReportRepository
{
  private readonly Supabase.Client _client;

  public ReportRepository(Supabase.Client client) => _client = client;

  public async Task<ReportOverview> GetReportOverviewAsync(ProductSalesFilter filter)
  {
    var result = await _client.Rpc<List<ReportOverview>>(
      "get_report_overview",
      new
      {
        p_start_date = filter.StartDate,
        p_end_date = filter.EndDate,
        p_category_name = string.IsNullOrWhiteSpace(filter.CategoryName) ? null : filter.CategoryName,
        p_product_name = string.IsNullOrWhiteSpace(filter.ProductName) ? null : filter.ProductName
      }
    );

    return result?.FirstOrDefault() ?? new ReportOverview();
  }

  public async Task<List<RevenueData>> GetCategoryRevenueAsync(ProductSalesFilter filter)
  {
    var result = await _client.Rpc<List<RevenueData>>(
      "get_category_revenue",
      new
      {
        p_start_date = filter.StartDate,
        p_end_date = filter.EndDate,
        p_category_name = string.IsNullOrWhiteSpace(filter.CategoryName) ? null : filter.CategoryName,
        p_product_name = string.IsNullOrWhiteSpace(filter.ProductName) ? null : filter.ProductName
      }
    );

    return result ?? [];
  }

  public async Task<List<ProfitData>> GetCategoryProfitAsync(DateTime startDate, DateTime endDate)
  {
    var result = await _client.Rpc<List<ProfitData>>(
      "get_category_profit",
      new
      {
        p_start_date = startDate,
        p_end_date = endDate
      }
    );

    return result ?? [];
  }

  public async Task<List<TopPerformingProduct>> GetTopPerformingProductsAsync(
    ProductSalesFilter filter,
    int limit = 5
  )
  {
    var result = await _client.Rpc<List<TopPerformingProduct>>(
      "get_top_performing_products",
      new
      {
        p_start_date = filter.StartDate,
        p_end_date = filter.EndDate,
        p_category_name = string.IsNullOrWhiteSpace(filter.CategoryName) ? null : filter.CategoryName,
        p_product_name = string.IsNullOrWhiteSpace(filter.ProductName) ? null : filter.ProductName,
        p_limit = limit
      }
    );

    return result ?? [];
  }

  public async Task<List<ProductSaleByDay>> GetProductSalesByDayAsync(
    DateTime startDate,
    DateTime endDate,
    string? categoryName = null,
    string? productName = null
  )
  {
    var result = await _client.Rpc<List<ProductSaleByDay>>(
      "get_product_sales_by_day",
      new
      {
        p_start_date = startDate,
        p_end_date = endDate,
        p_category_name = categoryName,
        p_product_name = productName
      }
    );

    return result ?? [];
  }

  public async Task<List<ProductSaleByWeek>> GetProductSalesByWeekAsync(
    DateTime startDate,
    DateTime endDate,
    string? categoryName = null,
    string? productName = null
  )
  {
    var result = await _client.Rpc<List<ProductSaleByWeek>>(
      "get_product_sales_by_week",
      new
      {
        p_start_date = startDate,
        p_end_date = endDate,
        p_category_name = categoryName,
        p_product_name = productName
      }
    );

    return result ?? [];
  }

  public async Task<List<ProductSaleByMonth>> GetProductSalesByMonthAsync(
    DateTime startDate,
    DateTime endDate,
    string? categoryName = null,
    string? productName = null
  )
  {
    var result = await _client.Rpc<List<ProductSaleByMonth>>(
      "get_product_sales_by_month",
      new
      {
        p_start_date = startDate,
        p_end_date = endDate,
        p_category_name = categoryName,
        p_product_name = productName
      }
    );

    return result ?? [];
  }

  public async Task<List<ProductSaleByYear>> GetProductSalesByYearAsync(
    DateTime startDate,
    DateTime endDate,
    string? categoryName = null,
    string? productName = null
  )
  {
    var result = await _client.Rpc<List<ProductSaleByYear>>(
      "get_product_sales_by_year",
      new
      {
        p_start_date = startDate,
        p_end_date = endDate,
        p_category_name = categoryName,
        p_product_name = productName
      }
    );

    return result ?? [];
  }
}
