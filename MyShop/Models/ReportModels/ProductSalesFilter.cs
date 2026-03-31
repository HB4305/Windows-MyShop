namespace MyShop.Models.ReportModels;

public class ProductSalesFilter
{
  public string Period { get; set; } = "day";
  public DateTime StartDate { get; set; } = DateTime.Now.Date.AddDays(-6);
  public DateTime EndDate { get; set; } = DateTime.Now.Date;
  public string? CategoryName { get; set; }
  public string? ProductName { get; set; }
}
