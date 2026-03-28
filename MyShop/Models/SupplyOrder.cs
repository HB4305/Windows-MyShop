using Postgrest.Attributes;
using Postgrest.Models;

namespace MyShop.Models;

[Table("supplyorders")]
public class SupplyOrder : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("supplier_id")]
    public int? SupplierId { get; set; }

    [Column("import_date")]
    public DateTime? ImportDate { get; set; }

    [Column("total_cost")]
    public decimal? TotalCost { get; set; }
}
