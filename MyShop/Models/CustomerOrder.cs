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
    public DateTime? CreatedAt { get; set; }

    [Column("customer_name")]
    [Required]
    public string CustomerName { get; set; } = string.Empty;

    [Column("customer_phone")]
    [Required]
    public string CustomerPhone { get; set; } = string.Empty;

    [Column("shipping_address")]
    public string? ShippingAddress { get; set; }

    [Column("order_type")]
    public string? OrderType { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [Column("payment_status")]
    public string? PaymentStatus { get; set; }

    [Column("total_amount")]
    public decimal? TotalAmount { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }
}
