using CommunityToolkit.Mvvm.ComponentModel;

namespace MyShop.Models;

public partial class SportItemVariant : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private int _sportItemId;

    [ObservableProperty]
    private string? _size;

    [ObservableProperty]
    private string? _color;

    [ObservableProperty]
    private int _stockQuantity;

    [ObservableProperty]
    private string? _sku;

    public string SizeText
    {
        get => Size ?? string.Empty;
        set => Size = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public string StockQuantityText
    {
        get => StockQuantity > 0 ? StockQuantity.ToString() : string.Empty;
        set
        {
            if (int.TryParse(value, out var q))
                StockQuantity = Math.Max(0, q);
            else
                StockQuantity = 0;
        }
    }

    public string DisplayText => $"Size: {Size ?? "N/A"}, Color: {Color ?? "N/A"} (Stock: {StockQuantity})";
}
