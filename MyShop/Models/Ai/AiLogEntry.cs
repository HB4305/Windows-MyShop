namespace MyShop.Models.Ai;

public class AiLogEntry
{
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string Operation { get; set; } = string.Empty;
    public long DurationMs { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? PromptSummary { get; set; }
    public string? ResponseSummary { get; set; }
}
