using System.ComponentModel.DataAnnotations;
using Postgrest.Attributes;
using Postgrest.Models;

namespace MyShop.Models;

[Table("suppliers")]
public class Supplier : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("name")]
    [Required]
    public string Name { get; set; } = string.Empty;

    [Column("contact_phone")]
    public string? ContactPhone { get; set; }

    [Column("supplier_type")]
    public string? SupplierType { get; set; }
}
