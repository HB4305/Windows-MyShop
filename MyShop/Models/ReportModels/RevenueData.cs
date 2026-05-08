using System.Text.Json.Serialization;

namespace MyShop.Models.ReportModels;

public class RevenueData
{
  [JsonPropertyName("date")]
  public DateTime Date { get; set; }

  [JsonPropertyName("gross_revenue")]
  public decimal GrossRevenue { get; set; }
}
