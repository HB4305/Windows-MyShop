namespace MyShop.Models;

public class LowStockAlert
{
    public string Name { get; set; } = string.Empty;
    public string? Size { get; set; }
    public string? Color { get; set; }
    public int? StockQuantity { get; set; }
}