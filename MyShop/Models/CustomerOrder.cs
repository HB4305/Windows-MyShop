using System.ComponentModel.DataAnnotations;

namespace MyShop.Models;

public class CustomerOrder
{
    public int Id { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    [Required(ErrorMessage = "Customer name is required")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string CustomerPhone { get; set; } = string.Empty;

    public string? ShippingAddress { get; set; }

    [RegularExpression("^(AtStore|Delivery)$", ErrorMessage = "Order type must be 'AtStore' or 'Delivery'")]
    public string? OrderType { get; set; }

    public string? Status { get; set; } = "Pending";

    public string? PaymentStatus { get; set; } = "Unpaid";

    [Range(0, 9999999999.99, ErrorMessage = "Total amount must be positive")]
    public decimal? TotalAmount { get; set; } = 0M;

    public string? Notes { get; set; } = string.Empty;
}
