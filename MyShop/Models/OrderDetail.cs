using System.ComponentModel.DataAnnotations;
using Postgrest.Attributes;
using Postgrest.Models;

namespace MyShop.Models;

[Table("orderdetails")]
public class OrderDetail : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("order_id")]
    public int? OrderId { get; set; }

    [Column("item_id")]
    public int? ItemId { get; set; }

    [Column("item_name")]
    [MaxLength(255, ErrorMessage = "Item name must not exceed 255 characters")]
    public string? ItemName { get; set; }

    [Column("quantity")]
    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }

    [Column("unit_price")]
    [Required(ErrorMessage = "Unit price is required")]
    [Range(0, 9999999999.99, ErrorMessage = "Unit price exceeds allowed range")]
    public decimal UnitPrice { get; set; }
}