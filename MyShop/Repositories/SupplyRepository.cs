using MyShop.Models;
using MyShop.Utils;

namespace MyShop.Repositories;

public class SupplyRepository
{
  private readonly Supabase.Client _client;

  public SupplyRepository(Supabase.Client client) => _client = client;

  public async Task<int> GetSuppliedProductCountByDateAsync(DateTime date)
  {
    var (start, end) = DateTimeUtils.GetDayRange(date);

    var supplyOrdersTask = _client.From<SupplyOrder>().Get();
    var supplyDetailsTask = _client.From<SupplyDetail>().Get();

    await Task.WhenAll(supplyOrdersTask, supplyDetailsTask);

    var supplyOrderIds = supplyOrdersTask.Result.Models
      .Where(order =>
        order.ImportDate.HasValue &&
        order.ImportDate.Value >= start &&
        order.ImportDate.Value < end)
      .Select(order => order.Id)
      .ToHashSet();

    return supplyDetailsTask.Result.Models
      .Where(detail => detail.SupplyId.HasValue && supplyOrderIds.Contains(detail.SupplyId.Value))
      .Where(detail => detail.ItemId.HasValue)
      .Select(detail => detail.ItemId!.Value)
      .Distinct()
      .Count();
  }
}
