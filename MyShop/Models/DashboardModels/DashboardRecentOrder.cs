namespace MyShop.Models.DashboardModels;

public class DashboardRecentOrder
{
    public int Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? Status { get; set; }
    public decimal? TotalPrice { get; set; }
    public List<DashboardRecentOrderDetail> Details { get; set; } = [];

    public class DashboardRecentOrderDetail
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? ProductName { get; set; }
        public string? Variant { get; set; }
    }

    // ── Computed display properties ─────────────────────────────────

    public string IdDisplay => $"EL-{Id}";
    public string TotalPriceDisplay => TotalPrice.HasValue ? $"${TotalPrice:N2}" : "$0.00";

    public string TimeAgo
    {
        get
        {
            if (!CreatedAt.HasValue) return "Unknown";
            var diff = DateTime.Now - CreatedAt.Value;
            if (diff.TotalMinutes < 1) return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hour{(diff.TotalHours >= 2 ? "s" : "")} ago";
            return $"{(int)diff.TotalDays} day{(diff.TotalDays >= 2 ? "s" : "")} ago";
        }
    }

    public string FirstItemLine
    {
        get
        {
            if (Details.Count == 0) return "";
            var d = Details[0];
            var qty = d.Quantity;
            var name = d.ProductName ?? $"Item #{d.Id}";
            var variant = string.IsNullOrEmpty(d.Variant) ? "" : $" ({d.Variant})";
            return $"{qty}x {name}{variant}";
        }
    }

    // Status badge: background hex color string
    public string StatusBadgeBg => Status?.ToUpperInvariant() switch
    {
        "PENDING" => "#FFF7ED",
        "SHIPPED" => "#EFF6FF",
        "DELIVERED" => "#ECFDF5",
        _ => "#F9FAFB"
    };

    // Status badge: foreground hex color string
    public string StatusBadgeFg => Status?.ToUpperInvariant() switch
    {
        "PENDING" => "#D97706",
        "SHIPPED" => "#2563EB",
        "DELIVERED" => "#059669",
        _ => "#6B7280"
    };
}
