using MyShop.Models;
using MyShop.Models.DashboardModels;
using MyShop.Repositories;

namespace MyShop.Services;

public class OrderService
{
  private readonly OrderRepository _repository;

  public OrderService(OrderRepository repository) => _repository = repository;

  public Task<int> GetOrderCountByDateAsync(DateTime? referenceTime = null)
  {
    DateTime date = referenceTime ?? DateTime.Now;
    return _repository.GetDateOrderCountAsync(date);
  }

  /// <summary>
  /// Order count on the day before the reference date (yesterday).
  /// </summary>
  public Task<int> GetPrevDayOrdersAsync(DateTime? referenceTime = null)
  {
    DateTime currentDate = referenceTime ?? DateTime.Now;
    DateTime prevDay = currentDate.AddDays(-1);
    return _repository.GetDateOrderCountAsync(prevDay);
  }

  public Task<decimal> GetRevenueByDateAsync(DateTime? referenceTime = null)
  {
    DateTime date = referenceTime ?? DateTime.Now;
    return _repository.GetDateRevenueAsync(date);
  }

  /// <summary>
  /// Revenue on the day before the reference date (yesterday).
  /// </summary>
  public Task<decimal> GetPrevDayRevenueAsync(DateTime? referenceTime = null)
  {
    DateTime currentDate = referenceTime ?? DateTime.Now;
    DateTime prevDay = currentDate.AddDays(-1);
    return _repository.GetDateRevenueAsync(prevDay);
  }

  public Task<List<DashboardRecentOrder>> GetRecentOrdersAsync(int limit = 3)
    => _repository.GetRecentOrdersAsync(limit);

  public Task<List<RevenueReport>> GetRevenuePointsAsync(DateTime start, DateTime end)
    => _repository.GetRevenuePointsAsync(start, end);

  public Task<List<RevenueReport>> GetMonthlyRevenueAsync(DateTime? referenceTime = null)
  {
    DateTime currentDate = referenceTime ?? DateTime.Now;
    DateTime start = new DateTime(currentDate.Year, currentDate.Month, 1);
    DateTime end = start.AddMonths(1);

    return _repository.GetRevenuePointsAsync(start, end);
  }

  public Task<List<DashboardTopSellerProduct>> GetTopSellingProductsAsync(
    int limit = 5,
    DateTime? referenceTime = null,
    int nDays = 7
  )
  {
    DateTime currentDate = referenceTime ?? DateTime.Now;
    DateTime currentEnd = currentDate.Date.AddDays(1);
    DateTime currentStart = currentEnd.AddDays(-nDays);
    DateTime prevEnd = currentStart;
    DateTime prevStart = prevEnd.AddDays(-nDays);

    return _repository.GetTopSellingProductsAsync(
      currentStart,
      currentEnd,
      prevStart,
      prevEnd,
      limit
    );
  }
}
