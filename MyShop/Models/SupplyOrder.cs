namespace MyShop.Models;

public class SupplyOrder
{
    public int Id { get; set; }
    public int? SupplierId { get; set; }
    public DateTime? ImportDate { get; set; }
    public decimal? TotalCost { get; set; }
}
