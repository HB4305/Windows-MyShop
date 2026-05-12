using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyShop.Models;

public partial class SportItem : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private int _categoryId;

    [ObservableProperty]
    [property: Required]
    private string _name = string.Empty;

    [ObservableProperty]
    private decimal? _costPrice;

    [ObservableProperty]
    private decimal? _sellingPrice;

    [ObservableProperty]
    private int? _stockQuantity;

    [ObservableProperty]
    private int? _lowStockThreshold;

    [ObservableProperty]
    [property: JsonPropertyName("image_urls")]
    private List<string> _imageUrls = new();

    [ObservableProperty]
    [property: JsonIgnore]
    private List<SportItemVariant> _variants = new();

    [ObservableProperty]
    private string? _description;

    [JsonIgnore]
    public int EffectiveStockQuantity =>
        Variants.Count > 0
            ? Variants.Sum(v => Math.Max(0, v.StockQuantity))
            : (StockQuantity ?? 0);

    [JsonIgnore]
    public string? PrimaryVariantSku => Variants.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v.Sku))?.Sku;

    /// <summary>Convenience for UI; must not be serialized — DB only has <c>image_urls</c>.</summary>
    [JsonIgnore]
    public string? ImageUrl => ImageUrls.FirstOrDefault();
}
