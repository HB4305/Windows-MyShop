using MyShop.Models.ReportModels;

namespace MyShop.Services.ReportStrategies;

public interface IProductSalesStrategy
{
  string Period { get; }
  Task<List<ProductSale>> GetSalesAsync(ProductSalesFilter filter);
}
