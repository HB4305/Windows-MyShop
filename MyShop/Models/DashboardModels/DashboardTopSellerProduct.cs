using System.Linq;
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

    [JsonProperty("image_urls")]
    public string[] ImageUrls { get; set; } = [];

    public string? ImageUrl => ImageUrls.FirstOrDefault();

    [JsonProperty("quantity_sold")]
    public int QuantitySold { get; set; }

    [JsonProperty("curr_period_revenue")]
    public decimal CurrPeriodRevenue { get; set; }

    [JsonProperty("prev_period_revenue")]
    public decimal PrevPeriodRevenue { get; set; }

    // ── Computed display properties ─────────────────────────────────
    public string QuantitySoldDisplay => QuantitySold.ToString("N0");
    public string RevenueDisplay => $"${CurrPeriodRevenue:N0}";
    public string SellingPriceDisplay => SellingPrice.HasValue ? $"${SellingPrice:N2}" : "N/A";
    public string CurrRevenueDisplay => $"${CurrPeriodRevenue:N2}";
    public string SubInfo => $"{CategoryName ?? "Unknown"} • {SellingPriceDisplay}";
    public string RevenueBadgeText => $"+${CurrPeriodRevenue:N2} this week";
}
