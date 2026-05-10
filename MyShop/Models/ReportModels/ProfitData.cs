using System.Text.Json.Serialization;

namespace MyShop.Models.ReportModels;

public class ProfitByCategory
{
  [JsonPropertyName("category_name")]
  public string CategoryName { get; set; } = string.Empty;

  [JsonPropertyName("profit")]
  public decimal Profit { get; set; }
}
