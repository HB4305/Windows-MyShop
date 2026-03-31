using MyShop.Repositories;

namespace MyShop.Services;

public class SupplyService
{
  private readonly SupplyRepository _repository;

  public SupplyService(SupplyRepository repository) => _repository = repository;

  public Task<int> GetSuppliedProductCountByDateAsync(DateTime? referenceTime = null)
  {
    DateTime date = referenceTime ?? DateTime.Now;
    return _repository.GetSuppliedProductCountByDateAsync(date);
  }

  /// <summary>
  /// Count distinct products supplied in the previous full month.
  /// </summary>
  public Task<int> GetSuppliedProductCountByMonthAsync(DateTime? referenceTime = null)
  {
    DateTime date = referenceTime ?? DateTime.Now;
    return _repository.GetSuppliedProductCountByMonthAsync(date);
  }
}
