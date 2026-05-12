using CommunityToolkit.Mvvm.ComponentModel;

namespace MyShop.Models.Ai;

public partial class AiChatMessage : ObservableObject
{
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    [ObservableProperty]
    private string _role = "user";

    [ObservableProperty]
    private string _content = string.Empty;

    public bool IsUser => Role.Equals("user", StringComparison.OrdinalIgnoreCase);
}
