using System.Linq;
using System.Text.Json.Serialization;

public class DashboardTopSellerProduct
{
    [JsonPropertyName("item_id")]
    public int ItemId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("category_name")]
    public string? CategoryName { get; set; }

    [JsonPropertyName("selling_price")]
    public decimal? SellingPrice { get; set; }

    [JsonPropertyName("image_urls")]
    public string[] ImageUrls { get; set; } = [];

    public string? ImageUrl => ImageUrls.FirstOrDefault();

    [JsonPropertyName("quantity_sold")]
    public int QuantitySold { get; set; }

    [JsonPropertyName("curr_period_revenue")]
    public decimal CurrPeriodRevenue { get; set; }

    [JsonPropertyName("prev_period_revenue")]
    public decimal PrevPeriodRevenue { get; set; }

    // ── Computed display properties ─────────────────────────────────
    public string QuantitySoldDisplay => QuantitySold.ToString("N0");
    public string RevenueDisplay => $"${CurrPeriodRevenue:N0}";
    public string SellingPriceDisplay => SellingPrice.HasValue ? $"${SellingPrice:N2}" : "N/A";
    public string CurrRevenueDisplay => $"${CurrPeriodRevenue:N2}";
    public string SubInfo => $"{CategoryName ?? "Unknown"} • {SellingPriceDisplay}";
    public string RevenueBadgeText => $"+${CurrPeriodRevenue:N2} this week";
}
