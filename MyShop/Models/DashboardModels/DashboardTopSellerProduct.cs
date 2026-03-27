using Newtonsoft.Json;

public class DashboardTopSellerProduct
{
  [JsonProperty("item_id")]
  public int ItemId { get; set; }

  [JsonProperty("name")]
  public string Name { get; set; } = string.Empty;

  [JsonProperty("category_name")]
  public string? CategoryName { get; set; }

  [JsonProperty("selling_price")]
  public decimal? SellingPrice { get; set; }

  [JsonProperty("image_url")]
  public string? ImageUrl { get; set; }

  [JsonProperty("quantity_sold")]
  public int QuantitySold { get; set; }

  [JsonProperty("curr_period_revenue")]
  public decimal CurrPeriodRevenue { get; set; }

  [JsonProperty("prev_period_revenue")]
  public decimal PrevPeriodRevenue { get; set; }
}