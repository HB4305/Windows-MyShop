using MyShop.Models;
using MyShop.Models.DashboardModels;
using MyShop.Utils;
using Postgrest;
using Sprache;

namespace MyShop.Repositories;

public class OrderRepository
{
  private readonly Supabase.Client _client;

  public OrderRepository(Supabase.Client client) => _client = client;

  public async Task<int> GetDateOrderCountAsync(DateTime date)
  {
    var (start, end) = DateTimeUtils.GetDayRange(date);
    var response = await _client.From<CustomerOrder>().Get();

    return response.Models.Count(order =>
      order.CreatedAt.HasValue &&
      order.CreatedAt.Value >= start &&
      order.CreatedAt.Value < end);
  }

  public async Task<int> GetAvgOrdersAsync(DateTime start, DateTime end)
  {
    var response = await _client.From<CustomerOrder>().Get();

    var totalOrders = response.Models.Count(order =>
      order.CreatedAt.HasValue &&
      order.CreatedAt.Value >= start &&
      order.CreatedAt.Value < end);

    var totalDays = Math.Max(1, (end.Date - start.Date).TotalDays);

    return (int)Math.Round(totalOrders / totalDays, MidpointRounding.AwayFromZero);
  }

  public async Task<decimal> GetDateRevenueAsync(DateTime date)
  {
    var (start, end) = DateTimeUtils.GetDayRange(date);

    var response = await _client
      .From<CustomerOrder>()
      .Where(order => order.Status == "Completed")
      .Get();

    return response.Models
      .Where(order =>
        order.CreatedAt.HasValue &&
        order.CreatedAt.Value >= start &&
        order.CreatedAt.Value < end)
      .Sum(order => order.TotalAmount ?? 0m);
  }

  public async Task<List<DashboardRecentOrder>> GetRecentOrdersAsync(int limit = 3)
  {
    var ordersTask = _client
      .From<CustomerOrder>()
      .Order(order => order.CreatedAt!, Constants.Ordering.Descending)
      .Limit(limit)
      .Get();

    var orderDetailsTask = _client.From<OrderDetail>().Get();

    await Task.WhenAll(ordersTask, orderDetailsTask);

    var detailsByOrderId = orderDetailsTask.Result.Models
      .Where(detail => detail.OrderId.HasValue)
      .GroupBy(detail => detail.OrderId!.Value)
      .ToDictionary(
        group => group.Key,
        group => group.Select(detail => new DashboardRecentOrder.DashboardRecentOrderDetail
        {
          Id = detail.Id,
          Quantity = detail.Quantity,
          UnitPrice = detail.UnitPrice
        }).ToList());

  return ordersTask.Result.Models
    .Select(order => new DashboardRecentOrder
    {
      Id = order.Id,
      CreatedAt = order.CreatedAt,
      CustomerName = order.CustomerName,
      Status = order.Status,
      TotalPrice = order.TotalAmount ?? 0m,
      Details = detailsByOrderId.GetValueOrDefault(order.Id, [])
    })
    .ToList();
  }

  public async Task<List<RevenueReport>> GetRevenuePointsAsync(DateTime start, DateTime end)
  {
    var response = await _client
      .From<RevenueReport>()
      .Order(report => report.Date, Constants.Ordering.Ascending)
      .Get();

    return response.Models
      .Where(report => report.Date >= start && report.Date < end)
      .ToList();
  }

  public async Task<List<DashboardTopSellerProduct>> GetTopSellingProductsAsync(
    DateTime currentStart,
    DateTime currentEnd,
    DateTime prevStart,
    DateTime prevEnd,
    int limit = 5
  ) {
    var response = await _client.Rpc<List<DashboardTopSellerProduct>>(
      "get_top_selling_products",
      new
      {
        p_start = currentStart,
        p_end = currentEnd,
        p_prev_start = prevStart,
        p_prev_end = prevEnd,
        p_limit = limit
      }
    );

    return response ?? [];
  }
}
