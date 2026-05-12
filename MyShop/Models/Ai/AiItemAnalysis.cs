using System.Text.Json.Serialization;

namespace MyShop.Models.Ai;

public class AiItemAnalysis
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    [JsonPropertyName("cost_price")]
    public decimal? CostPrice { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("low_stock_threshold")]
    public int? LowStockThreshold { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("suggested_variants")]
    public List<AiVariantSuggestion> SuggestedVariants { get; set; } = new();

    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    [JsonPropertyName("reasons")]
    public List<string> Reasons { get; set; } = new();

    [JsonPropertyName("field_confidence")]
    public Dictionary<string, double> FieldConfidence { get; set; } = new();

    [JsonPropertyName("field_reasons")]
    public Dictionary<string, string> FieldReasons { get; set; } = new();
}

public class AiVariantSuggestion
{
    [JsonPropertyName("size")]
    public string? Size { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("sku")]
    public string? Sku { get; set; }
}
