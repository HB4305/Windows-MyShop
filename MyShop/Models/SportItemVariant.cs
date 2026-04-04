using System.ComponentModel.DataAnnotations;

namespace MyShop.Models;

public class SportItemVariant
{
    public int Id { get; set; }

    public int SportItemId { get; set; }

    public string? Size { get; set; }

    public string? Color { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    public string? Sku { get; set; }
}
