using System.ComponentModel.DataAnnotations;

namespace MyShop.Models;

public class SportItemVariant
{
    public int Id { get; set; }

    public int SportItemId { get; set; }

    public string? Size { get; set; }

    /// <summary>String wrapper for Size — supports both numeric and alphanumeric input (e.g., "9", "XL").</summary>
    public string SizeText
    {
        get => Size ?? string.Empty;
        set => Size = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public string? Color { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    /// <summary>String wrapper for StockQuantity — used for TextBox binding with numeric-only input.</summary>
    public string StockQuantityText
    {
        get => StockQuantity > 0 ? StockQuantity.ToString() : string.Empty;
        set
        {
            if (int.TryParse(value, out var q))
                StockQuantity = Math.Max(0, q);
            else
                StockQuantity = 0;
        }
    }

    public string? Sku { get; set; }
}
