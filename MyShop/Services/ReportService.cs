using MyShop.Models.ReportModels;
using MyShop.Repositories;

namespace MyShop.Services;

public class ReportService
{
  private readonly ReportRepository _repository;
  public ReportService(ReportRepository repository) => _repository = repository;

  public static PeriodSelection CreatePeriodSelection(ReportPeriod period, DateTime? referenceDate = null)
  {
    return period switch
    {
      ReportPeriod.Week => new WeekPeriodSelection(referenceDate),
      ReportPeriod.Month => new MonthPeriodSelection(referenceDate),
      ReportPeriod.Year => new YearPeriodSelection(referenceDate),
      _ => new WeekPeriodSelection(referenceDate)
    };
  }

  public async Task<ReportOverview> GetReportOverviewAsync(PeriodSelection periodSelection)
  {
    var currentRange = periodSelection.Range;
    var previousRange = GetPreviousRange(periodSelection);

    var current = await _repository.GetOverviewSnapshotAsync(currentRange.Start, currentRange.End);
    var previous = await _repository.GetOverviewSnapshotAsync(previousRange.Start, previousRange.End);

    return new ReportOverview
    {
      Revenue = current.Revenue,
      PreviousRevenue = previous.Revenue,
      QuantitySold = current.QuantitySold,
      PreviousQuantitySold = previous.QuantitySold,
      Profit = current.Profit,
      PreviousProfit = previous.Profit,
      CustomersCount = current.CustomersCount,
      PreviousCustomersCount = previous.CustomersCount
    };
  }

  public Task<List<SoldQuantityData>> GetProductSalesAsync(ProductSalesFilter filter, PeriodSelection periodSelection)
  {
    var range = periodSelection.Range;
    return _repository.GetSoldQuantityDataAsync(
      range.Start,
      range.End,
      filter.CategoryName,
      filter.ProductName
    );
  }

  public async Task<List<RevenueData>> GetRevenueDataAsync(PeriodSelection periodSelection)
  {
    var range = periodSelection.Range;
    return await _repository.GetRevenueDataAsync(
      range.Start,
      range.End
    );
  }

  public Task<List<ProfitByCategory>> GetProfitDataAsync(PeriodSelection periodSelection)
  {
    var range = periodSelection.Range;
    return _repository.GetProfitByCategoryAsync(range.Start, range.End);
  }

  public Task<List<TopPerformingProduct>> GetTopPerformingProductsAsync(PeriodSelection periodSelection, int limit = 5)
  {
    var range = periodSelection.Range;
    return _repository.GetTopPerformingProductsAsync(range.Start, range.End, limit);
  }

  private static (DateTime Start, DateTime End) GetPreviousRange(PeriodSelection currentSelection)
  {
    var currentRange = currentSelection.Range;

    return currentSelection.Period switch
    {
      ReportPeriod.Week => (currentRange.Start.AddDays(-7), currentRange.Start),
      ReportPeriod.Month => (currentRange.Start.AddMonths(-1), currentRange.Start),
      ReportPeriod.Year => (currentRange.Start.AddYears(-1), currentRange.Start),
      _ => (currentRange.Start, currentRange.End)
    };
  }
}
