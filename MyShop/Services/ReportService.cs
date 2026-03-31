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
