using Postgrest.Attributes;
using Postgrest.Models;

namespace MyShop.Models;

[Table("view_revenue_report")]
public class RevenueReport : BaseModel
{
    [PrimaryKey("date", false)]
    [Column("date")]
    public DateTime Date { get; set; }

    [Column("total_orders")]
    public int TotalOrders { get; set; }

    [Column("gross_revenue")]
    public decimal? GrossRevenue { get; set; }
}
