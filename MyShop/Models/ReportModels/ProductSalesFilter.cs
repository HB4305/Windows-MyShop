namespace MyShop.Models.ReportModels;

public class ProductSalesFilter
{
  public ReportPeriod Period { get; set; } = ReportPeriod.Week;
  public string? CategoryName { get; set; }
  public string? ProductName { get; set; }
}
