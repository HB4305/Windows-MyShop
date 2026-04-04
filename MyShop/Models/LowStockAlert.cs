namespace MyShop.Models;

public class LowStockAlert
{
    public string Name { get; set; } = string.Empty;
    public int? StockQuantity { get; set; }
}