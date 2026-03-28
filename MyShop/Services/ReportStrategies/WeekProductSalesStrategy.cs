using MyShop.Models.ReportModels;
using MyShop.Repositories;

namespace MyShop.Services.ReportStrategies;

public class WeekProductSalesStrategy : IProductSalesStrategy
{
  private readonly ReportRepository _repository;

  public WeekProductSalesStrategy(ReportRepository repository)
  {
    _repository = repository;
  }

  public string Period => "week";

  public async Task<List<ProductSale>> GetSalesAsync(ProductSalesFilter filter)
    => (await _repository.GetProductSalesByWeekAsync(
      filter.StartDate,
      filter.EndDate,
      filter.CategoryName,
      filter.ProductName
    )).Cast<ProductSale>().ToList();
}
