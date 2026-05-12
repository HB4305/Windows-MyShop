using CommunityToolkit.Mvvm.ComponentModel;
using MyShop.Models;
using System.Globalization;

namespace MyShop.Models.Ai;

public partial class AiBulkSuggestion : ObservableObject
{
    public required SportItemListRow Row { get; init; }

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private string _suggestedDescription = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SuggestedPriceDisplay))]
    private decimal? _suggestedPrice;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConfidenceDisplay))]
    private double _confidence;

    [ObservableProperty]
    private string _status = "Pending";

    [ObservableProperty]
    private string _error = string.Empty;

    public string SuggestedPriceDisplay => SuggestedPrice.HasValue
        ? $"Suggested price: {SuggestedPrice.Value.ToString("C", CultureInfo.InvariantCulture)}"
        : "Suggested price: N/A";

    public string ConfidenceDisplay => $"Confidence: {Confidence.ToString("P0", CultureInfo.InvariantCulture)}";
}
