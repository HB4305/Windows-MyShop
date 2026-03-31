using Newtonsoft.Json;

namespace MyShop.Models.ReportModels;

public class ProfitData
{
  [JsonProperty("category_name")]
  public string CategoryName { get; set; } = string.Empty;

  [JsonProperty("profit")]
  public decimal Profit { get; set; }

  public string ProfitDisplay => $"{Profit:N0} VND";
}
