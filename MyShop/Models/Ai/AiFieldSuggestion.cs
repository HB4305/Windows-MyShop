using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;

namespace MyShop.Models.Ai;

public partial class AiFieldSuggestion : ObservableObject
{
    [ObservableProperty]
    private string _key = string.Empty;

    [ObservableProperty]
    private string _label = string.Empty;

    [ObservableProperty]
    private string _value = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConfidenceDisplay))]
    private double _confidence;

    [ObservableProperty]
    private string _reason = string.Empty;

    [ObservableProperty]
    private bool _isAccepted;

    public string ConfidenceLevel => Confidence switch
    {
        >= 0.9 => "High Accuracy",
        >= 0.7 => "Reliable",
        >= 0.5 => "Average",
        _ => "Needs Review"
    };

    public string ConfidenceColor => Confidence switch
    {
        >= 0.9 => "#16A34A", // Green
        >= 0.7 => "#2563EB", // Blue
        >= 0.5 => "#CA8A04", // Yellow/Gold
        _ => "#DC2626"       // Red
    };

    public string ConfidenceDisplay => $"{ConfidenceLevel} ({Confidence.ToString("P0", CultureInfo.InvariantCulture)})";
}
