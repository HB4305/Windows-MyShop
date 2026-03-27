using Postgrest.Attributes;
using Postgrest.Models;

namespace MyShop.Models;

[Table("view_low_stock_alert")]
public class LowStockAlert : BaseModel
{
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("size")]
    public string? Size { get; set; }

    [Column("color")]
    public string? Color { get; set; }

    [Column("stock_quantity")]
    public int? StockQuantity { get; set; }
}
