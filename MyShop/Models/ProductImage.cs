using System.ComponentModel.DataAnnotations;
using Postgrest.Attributes;
using Postgrest.Models;

namespace MyShop.Models;

[Table("product_images")]
public class ProductImage : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("item_id")]
    public int? ItemId { get; set; }

    [Column("image_url")]
    [Required]
    public string ImageUrl { get; set; } = string.Empty;
}
