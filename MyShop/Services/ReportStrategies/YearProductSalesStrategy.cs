using MyShop.Models.ReportModels;
using MyShop.Repositories;

namespace MyShop.Services.ReportStrategies;

public class YearProductSalesStrategy : IProductSalesStrategy
{
  private readonly ReportRepository _repository;

  public YearProductSalesStrategy(ReportRepository repository)
  {
    _repository = repository;
  }

  public string Period => "year";

  public async Task<List<ProductSale>> GetSalesAsync(ProductSalesFilter filter)
    => (await _repository.GetProductSalesByYearAsync(
      filter.StartDate,
      filter.EndDate,
      filter.CategoryName,
      filter.ProductName
    )).Cast<ProductSale>().ToList();
}
