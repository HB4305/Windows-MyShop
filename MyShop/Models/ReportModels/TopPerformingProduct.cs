using Newtonsoft.Json;

namespace MyShop.Models.ReportModels;

public class TopPerformingProduct
{
  [JsonProperty("id")]
  public int Id { get; set; } = 0;

  [JsonProperty("product_name")]
  public string ProductName { get; set; } = string.Empty;

  [JsonProperty("category_name")]
  public string CategoryName { get; set; } = string.Empty;

  [JsonProperty("image_url")]
  public string ImageUrl { get; set; } = string.Empty;

  [JsonProperty("total_quantity_sold")]
  public int TotalQuantitySold { get; set; }

  [JsonProperty("gross_revenue")]
  public decimal GrossRevenue { get; set; }

  [JsonProperty("profit")]
  public decimal Profit { get; set; }
}
