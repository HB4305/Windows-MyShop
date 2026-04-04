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

    public string? Sku { get; set; }

    public string? Size { get; set; }

    public string? Color { get; set; }

    public decimal? CostPrice { get; set; }

    public decimal? SellingPrice { get; set; }

    public int? StockQuantity { get; set; }

    public int? LowStockThreshold { get; set; }

    [JsonProperty("image_urls")]
    public List<string> ImageUrls { get; set; } = new();

    /// <summary>Convenience for UI; must not be serialized — DB only has <c>image_urls</c>.</summary>
    [JsonIgnore]
    public string? ImageUrl => ImageUrls.FirstOrDefault();
}
