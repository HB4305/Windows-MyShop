using System.ComponentModel.DataAnnotations;

namespace MyShop.Models;

public class SupplyDetail
{
    public int Id { get; set; }
    public int? SupplyId { get; set; }
    public int? ItemId { get; set; }
    public long? VariantId { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public decimal ImportPrice { get; set; }
}
