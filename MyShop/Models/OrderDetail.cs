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

    [Column("quantity")]
    [Required]
    public int Quantity { get; set; }

    [Column("unit_price")]
    [Required]
    public decimal UnitPrice { get; set; }
}
