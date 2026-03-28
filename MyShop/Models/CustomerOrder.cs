using System.ComponentModel.DataAnnotations;
using Postgrest.Attributes;
using Postgrest.Models;

namespace MyShop.Models;

[Table("customerorders")]
public class CustomerOrder : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [Column("customer_name")]
    [Required(ErrorMessage = "Customer name is required")]
    public string CustomerName { get; set; }

    [Column("customer_phone")]
    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string CustomerPhone { get; set; }

    [Column("shipping_address")]
    public string? ShippingAddress { get; set; }

    [Column("order_type")]
    [RegularExpression("^(AtStore|Delivery)$", ErrorMessage = "Order type must be 'AtStore' or 'Delivery'")]
    public string? OrderType { get; set; }

    [Column("status")]
    public string? Status { get; set; } = "Pending";

    [Column("payment_status")]
    public string? PaymentStatus { get; set; } = "Unpaid";

    [Column("total_amount")]
    [Range(0, 9999999999.99, ErrorMessage = "Total amount must be positive")]
    public decimal? TotalAmount { get; set; } = 0M;     

    [Column("notes")]
    public string? Notes { get; set; } = string.Empty;
}