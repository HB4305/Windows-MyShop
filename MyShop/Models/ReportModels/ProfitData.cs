using Newtonsoft.Json;

namespace MyShop.Models.ReportModels;

public class ProfitByCategory
{
  [JsonProperty("category_name")]
  public string CategoryName { get; set; } = string.Empty;

  [JsonProperty("profit")]
  public decimal Profit { get; set; }
}
