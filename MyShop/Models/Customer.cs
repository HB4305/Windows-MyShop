using System.ComponentModel.DataAnnotations;

namespace MyShop.Models;

public class Customer
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    public string? Address { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    // Calculated fields for UI
    public decimal TotalSpent { get; set; }
    public int OrderCount { get; set; }
}
