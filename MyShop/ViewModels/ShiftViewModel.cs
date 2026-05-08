using MyShop.Models;
using MyShop.Services;
using System.Globalization;

namespace MyShop.ViewModels;

public partial class ShiftViewModel : ObservableObject
{
    private readonly ShiftService _shiftService;
    private readonly CurrentUserService _currentUserService;

    public ShiftViewModel(ShiftService shiftService, CurrentUserService currentUserService)
    {
        _shiftService = shiftService;
        _currentUserService = currentUserService;
    }

    [ObservableProperty] private Shift? _activeShift;
    [ObservableProperty] private decimal _startingCash;
    [ObservableProperty] private string _startingCashText = string.Empty;
    [ObservableProperty] private decimal _actualCashTotal;
    [ObservableProperty] private string _actualCashTotalText = string.Empty;
    [ObservableProperty] private decimal _expectedCash;
    [ObservableProperty] private string _notes = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isBusy;

    public decimal Discrepancy => ActualCashTotal - ExpectedCash;
    public bool HasActiveShift => ActiveShift is not null;

    [RelayCommand]
    private async Task LoadActiveShiftAsync()
    {
        if (!_currentUserService.UserId.HasValue)
        {
            StatusMessage = "Please sign in first.";
            return;
        }

        try
        {
            IsBusy = true;
            ActiveShift = await _shiftService.GetActiveShiftAsync(_currentUserService.UserId.Value);
            if (ActiveShift is not null)
            {
                ExpectedCash = await _shiftService.GetExpectedCashAsync(ActiveShift.Id);
            }
            else
            {
                ExpectedCash = 0m;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load shift: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(HasActiveShift));
            OnPropertyChanged(nameof(Discrepancy));
        }
    }

    [RelayCommand]
    private async Task OpenShiftAsync()
    {
        if (!_currentUserService.UserId.HasValue)
        {
            StatusMessage = "Please sign in first.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = string.Empty;
            ActiveShift = await _shiftService.OpenShiftAsync(_currentUserService.UserId.Value, StartingCash);
            ExpectedCash = 0m;
            ActualCashTotal = 0m;
            ActualCashTotalText = string.Empty;
            Notes = string.Empty;
            StatusMessage = $"Shift #{ActiveShift.Id} opened successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to open shift: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(HasActiveShift));
            OnPropertyChanged(nameof(Discrepancy));
        }
    }

    [RelayCommand]
    private async Task CloseShiftAsync()
    {
        if (ActiveShift is null)
        {
            StatusMessage = "No active shift to close.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = string.Empty;

            var (closedShift, expectedCash) = await _shiftService.CloseShiftAsync(
                ActiveShift.Id,
                ActualCashTotal,
                string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim());

            ExpectedCash = expectedCash;
            ActiveShift = null;
            StatusMessage = $"Shift #{closedShift.Id} closed successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to close shift: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(HasActiveShift));
            OnPropertyChanged(nameof(Discrepancy));
        }
    }

    partial void OnStartingCashTextChanged(string value) => StartingCash = ParseDecimalOrZero(value);

    partial void OnActualCashTotalTextChanged(string value)
    {
        ActualCashTotal = ParseDecimalOrZero(value);
        OnPropertyChanged(nameof(Discrepancy));
    }

    partial void OnActualCashTotalChanged(decimal value) => OnPropertyChanged(nameof(Discrepancy));

    partial void OnExpectedCashChanged(decimal value) => OnPropertyChanged(nameof(Discrepancy));

    private static decimal ParseDecimalOrZero(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return 0m;
        }

        if (decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.CurrentCulture, out var currentCultureValue))
        {
            return currentCultureValue;
        }

        if (decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariantValue))
        {
            return invariantValue;
        }

        return 0m;
    }
}
