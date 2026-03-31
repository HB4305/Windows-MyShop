using Newtonsoft.Json;

namespace MyShop.Models.ReportModels;

public class ReportOverview
{
  [JsonProperty("total_revenue")]
  public decimal TotalRevenue { get; set; }

  [JsonProperty("total_quantity_sold")]
  public int TotalQuantitySold { get; set; }

  [JsonProperty("total_profit")]
  public decimal TotalProfit { get; set; }

  [JsonProperty("total_customers")]
  public int TotalCustomers { get; set; }
}
