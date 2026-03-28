using MyShop.Models.ReportModels;
using MyShop.Repositories;

namespace MyShop.Services.ReportStrategies;

public class DayProductSalesStrategy : IProductSalesStrategy
{
  private readonly ReportRepository _repository;

  public DayProductSalesStrategy(ReportRepository repository)
  {
    _repository = repository;
  }

  public string Period => "day";

  public async Task<List<ProductSale>> GetSalesAsync(ProductSalesFilter filter)
  {
    var result = await _repository.GetProductSalesByDayAsync(
      filter.StartDate,
      filter.EndDate,
      filter.CategoryName,
      filter.ProductName
    );

    return result.Cast<ProductSale>().ToList();
  }
}
