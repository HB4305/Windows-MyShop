using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace MyShop.Views.Dialogs;

public sealed partial class NegativeDiscrepancyReasonDialog : ContentDialog, INotifyPropertyChanged
{
    private string _reason = string.Empty;
    private string _validationMessage = string.Empty;

    public NegativeDiscrepancyReasonDialog(decimal discrepancy, string? existingReason = null)
    {
        InitializeComponent();
        Discrepancy = discrepancy;
        Reason = existingReason ?? string.Empty;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public decimal Discrepancy { get; }

    public string DiscrepancyMessage =>
        $"The drawer is short by {Math.Abs(Discrepancy).ToString("C", CultureInfo.CurrentCulture)}. A clear note is required before the shift report can be submitted.";

    public string Reason
    {
        get => _reason;
        set
        {
            SetProperty(ref _reason, value);
            Validate();
        }
    }

    public string ValidationMessage
    {
        get => _validationMessage;
        private set => SetProperty(ref _validationMessage, value);
    }

    public ContentDialogResult Result { get; private set; } = ContentDialogResult.None;

    public string ReasonText => Reason.Trim();

    public new async Task<ContentDialogResult> ShowAsync()
    {
        await base.ShowAsync();
        return Result;
    }

    private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
    {
        Validate();
        if (!string.IsNullOrEmpty(ValidationMessage))
        {
            return;
        }

        Result = ContentDialogResult.Primary;
        Hide();
    }

    private void CancelBtn_Click(object sender, RoutedEventArgs e)
    {
        Result = ContentDialogResult.None;
        Hide();
    }

    private void Validate()
    {
        ValidationMessage = Reason.Trim().Length >= 10
            ? string.Empty
            : "Please provide a clear reason with at least 10 characters.";
    }

    private void SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(storage, value))
        {
            return;
        }

        storage = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
