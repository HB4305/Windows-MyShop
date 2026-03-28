using MyShop.Models.ReportModels;
using MyShop.Repositories;

namespace MyShop.Services.ReportStrategies;

public class MonthProductSalesStrategy : IProductSalesStrategy
{
  private readonly ReportRepository _repository;

  public MonthProductSalesStrategy(ReportRepository repository)
  {
    _repository = repository;
  }

  public string Period => "month";

  public async Task<List<ProductSale>> GetSalesAsync(ProductSalesFilter filter)
    => (await _repository.GetProductSalesByMonthAsync(
      filter.StartDate,
      filter.EndDate,
      filter.CategoryName,
      filter.ProductName
    )).Cast<ProductSale>().ToList();
}
