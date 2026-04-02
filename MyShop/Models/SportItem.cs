using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Postgrest.Attributes;
using Postgrest.Models;

namespace MyShop.Models;

[Table("sportitems")]
public class SportItem : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("name")]
    [Required]
    public string Name { get; set; } = string.Empty;

    [Column("sku")]
    public string? Sku { get; set; }

    [Column("size")]
    public string? Size { get; set; }

    [Column("color")]
    public string? Color { get; set; }

    [Column("cost_price")]
    public decimal? CostPrice { get; set; }

    [Column("selling_price")]
    public decimal? SellingPrice { get; set; }

    [Column("stock_quantity")]
    public int? StockQuantity { get; set; }

    [Column("low_stock_threshold")]
    public int? LowStockThreshold { get; set; }

    [Column("image_urls")]
    [JsonProperty("image_urls")]
    public List<string> ImageUrls { get; set; } = new();

    /// <summary>Convenience for UI; must not be serialized — PostgREST only has <c>image_urls</c>.</summary>
    [JsonIgnore]
    public string? ImageUrl => ImageUrls.FirstOrDefault();
}
