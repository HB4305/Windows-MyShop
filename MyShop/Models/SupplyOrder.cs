namespace MyShop.Models;

public class SupplyOrder
{
    public int Id { get; set; }
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public DateTime? ImportDate { get; set; }
    public decimal? TotalCost { get; set; }

    public string IdDisplay => $"#{Id}";
    public string ImportDateDisplay => ImportDate?.ToString("dd/MM/yyyy HH:mm") ?? "";
}
