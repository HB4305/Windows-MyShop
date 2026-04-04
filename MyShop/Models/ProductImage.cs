using System.ComponentModel.DataAnnotations;

namespace MyShop.Models;

public class ProductImage
{
    public int Id { get; set; }
    public int? ItemId { get; set; }

    [Required]
    public string ImageUrl { get; set; } = string.Empty;
}
