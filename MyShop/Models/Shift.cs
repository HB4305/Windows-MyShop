using System.ComponentModel.DataAnnotations;

namespace MyShop.Models;

public class Shift
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTimeOffset? StartTime { get; set; }

    public DateTimeOffset? EndTime { get; set; }

    [Range(0, 9999999999.99, ErrorMessage = "Starting cash must be positive")]
    public decimal StartingCash { get; set; }

    [Range(0, 9999999999.99, ErrorMessage = "Actual cash total must be positive")]
    public decimal ActualCashTotal { get; set; }

    public string? Notes { get; set; }

    public string Status { get; set; } = "Open";
}
