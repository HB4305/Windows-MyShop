using System.ComponentModel.DataAnnotations;
using Postgrest.Attributes;
using Postgrest.Models;

namespace MyShop.Models;

[Table("supplydetails")]
public class SupplyDetail : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("supply_id")]
    public int? SupplyId { get; set; }

    [Column("item_id")]
    public int? ItemId { get; set; }

    [Column("quantity")]
    [Required]
    public int Quantity { get; set; }

    [Column("import_price")]
    [Required]
    public decimal ImportPrice { get; set; }
}
