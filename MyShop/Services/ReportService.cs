using MyShop.Models.ReportModels;
using MyShop.Repositories;
using MyShop.Services.ReportStrategies;

namespace MyShop.Services;

public class ReportService
{
  private readonly ReportRepository _repository;
  private readonly IReadOnlyDictionary<string, IProductSalesStrategy> _strategies;

  public ReportService(
    ReportRepository repository,
    IEnumerable<IProductSalesStrategy> strategies
  )
  {
    _repository = repository;
    _strategies = strategies.ToDictionary(strategy => strategy.Period, StringComparer.OrdinalIgnoreCase);
  }

  public async Task<decimal> GetTotalRevenueAsync(ProductSalesFilter filter)
    => (await GetReportOverviewAsync(filter)).TotalRevenue;

  public async Task<int> GetTotalQuantitySoldAsync(ProductSalesFilter filter)
    => (await GetReportOverviewAsync(filter)).TotalQuantitySold;

  public async Task<decimal> GetTotalProfitAsync(ProductSalesFilter filter)
    => (await GetReportOverviewAsync(filter)).TotalProfit;

  public async Task<int> GetTotalCustomersAsync(ProductSalesFilter filter)
    => (await GetReportOverviewAsync(filter)).TotalCustomers;

  public Task<ReportOverview> GetReportOverviewAsync(ProductSalesFilter filter)
  {
    filter.StartDate = NormalizeStartDate(filter.StartDate);
    filter.EndDate = NormalizeEndDate(filter.EndDate);
    filter.CategoryName = NormalizeFilter(filter.CategoryName);
    filter.ProductName = NormalizeFilter(filter.ProductName);

    return _repository.GetReportOverviewAsync(filter);
  }

  public Task<List<RevenueData>> GetCategoryRevenueAsync(ProductSalesFilter filter)
  {
    filter.StartDate = NormalizeStartDate(filter.StartDate);
    filter.EndDate = NormalizeEndDate(filter.EndDate);
    filter.CategoryName = NormalizeFilter(filter.CategoryName);
    filter.ProductName = NormalizeFilter(filter.ProductName);

    return _repository.GetCategoryRevenueAsync(filter);
  }

  public Task<List<ProfitData>> GetCurrentCategoryProfitAsync(string period, DateTime? referenceTime = null)
  {
    var current = referenceTime ?? DateTime.Now;
    var normalizedPeriod = NormalizeFilter(period)?.ToLowerInvariant() ?? "day";

    DateTime startDate;
    DateTime endDate;

    switch (normalizedPeriod)
    {
      case "week":
        var dayOfWeek = (int)current.DayOfWeek;
        var offset = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
        startDate = current.Date.AddDays(-offset);
        endDate = startDate.AddDays(7);
        break;
      case "month":
        startDate = new DateTime(current.Year, current.Month, 1);
        endDate = startDate.AddMonths(1);
        break;
      case "year":
        startDate = new DateTime(current.Year, 1, 1);
        endDate = startDate.AddYears(1);
        break;
      default:
        startDate = current.Date;
        endDate = startDate.AddDays(1);
        break;
    }

    return _repository.GetCategoryProfitAsync(startDate, endDate);
  }

  public Task<List<ProductSale>> GetProductSalesInPeriodAsync(ProductSalesFilter filter)
  {
    filter.StartDate = NormalizeStartDate(filter.StartDate);
    filter.EndDate = NormalizeEndDate(filter.EndDate);
    filter.CategoryName = NormalizeFilter(filter.CategoryName);
    filter.ProductName = NormalizeFilter(filter.ProductName);

    if (_strategies.TryGetValue(filter.Period, out var strategy))
    {
      return strategy.GetSalesAsync(filter);
    }

    return _strategies["day"].GetSalesAsync(filter);
  }

  public Task<List<TopPerformingProduct>> GetTopPerformingProductsAsync(
    ProductSalesFilter filter,
    int limit = 5
  )
  {
    filter.StartDate = NormalizeStartDate(filter.StartDate);
    filter.EndDate = NormalizeEndDate(filter.EndDate);
    filter.CategoryName = NormalizeFilter(filter.CategoryName);
    filter.ProductName = NormalizeFilter(filter.ProductName);

    return _repository.GetTopPerformingProductsAsync(filter, limit);
  }

  private static DateTime NormalizeStartDate(DateTime value)
    => value.Date;

  private static DateTime NormalizeEndDate(DateTime value)
    => value.Date.AddDays(1);

  private static string? NormalizeFilter(string? value)
    => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
