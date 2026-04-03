using System.ComponentModel.DataAnnotations;

namespace MyShop.Models;

public class Supplier
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? ContactPhone { get; set; }

    public string? SupplierType { get; set; }
}
