using System.Linq;
using System.Text.Json.Serialization;

namespace MyShop.Models.ReportModels;

public class TopPerformingProduct
{
  [JsonPropertyName("id")]
  public int Id { get; set; } = 0;

  [JsonPropertyName("product_name")]
  public string ProductName { get; set; } = string.Empty;

  [JsonPropertyName("category_name")]
  public string CategoryName { get; set; } = string.Empty;

  [JsonPropertyName("image_urls")]
  public string[] ImageUrls { get; set; } = [];

  public string ImageUrl => ImageUrls.FirstOrDefault() ?? string.Empty;

  [JsonPropertyName("total_quantity_sold")]
  public int TotalQuantitySold { get; set; }

  [JsonPropertyName("gross_revenue")]
  public decimal GrossRevenue { get; set; }

  [JsonPropertyName("profit")]
  public decimal Profit { get; set; }
}
