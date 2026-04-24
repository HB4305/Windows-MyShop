using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace MyShop.Models;

public class SportItem
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public decimal? CostPrice { get; set; }

    public decimal? SellingPrice { get; set; }

    public int? StockQuantity { get; set; }

    public int? LowStockThreshold { get; set; }

    [JsonProperty("image_urls")]
    public List<string> ImageUrls { get; set; } = new();

    [JsonIgnore]
    public List<SportItemVariant> Variants { get; set; } = new();

    [JsonIgnore]
    public int EffectiveStockQuantity =>
        Variants.Count > 0
            ? Variants.Sum(v => Math.Max(0, v.StockQuantity))
            : (StockQuantity ?? 0);

    [JsonIgnore]
    public string? PrimaryVariantSku => Variants.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v.Sku))?.Sku;

    public string? Description { get; set; }

    /// <summary>Convenience for UI; must not be serialized — DB only has <c>image_urls</c>.</summary>
    [JsonIgnore]
    public string? ImageUrl => ImageUrls.FirstOrDefault();
}
