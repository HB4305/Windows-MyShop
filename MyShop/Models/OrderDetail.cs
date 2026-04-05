using System.ComponentModel.DataAnnotations;

namespace MyShop.Models;

public class OrderDetail
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public int? ItemId { get; set; }

    [MaxLength(255, ErrorMessage = "Item name must not exceed 255 characters")]
    public string? ItemName { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Unit price is required")]
    [Range(0, 9999999999.99, ErrorMessage = "Unit price exceeds allowed range")]
    public decimal UnitPrice { get; set; }

    public string TotalPriceDisplay => (Quantity * UnitPrice).ToString("C");
}
