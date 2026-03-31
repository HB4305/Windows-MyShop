using Newtonsoft.Json;

namespace MyShop.Models.ReportModels;

public class RevenueData
{
  [JsonProperty("category_name")]
  public string CategoryName { get; set; } = string.Empty;

  [JsonProperty("gross_revenue")]
  public decimal GrossRevenue { get; set; }

  public string GrossRevenueDisplay => $"{GrossRevenue:N0} VND";
}
