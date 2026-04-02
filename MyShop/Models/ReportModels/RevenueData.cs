using Newtonsoft.Json;

namespace MyShop.Models.ReportModels;

public class RevenueData
{
  [JsonProperty("date")]
  public DateTime Date { get; set; }

  [JsonProperty("gross_revenue")]
  public decimal GrossRevenue { get; set; }
}
