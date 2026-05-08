using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class ShiftReportLogsViewModel : ObservableObject
{
    private readonly ShiftService _shiftService;
    private readonly CurrentUserService _currentUserService;

    public ShiftReportLogsViewModel(ShiftService shiftService, CurrentUserService currentUserService)
    {
        _shiftService = shiftService;
        _currentUserService = currentUserService;
    }

    [ObservableProperty] private ObservableCollection<ShiftReportLogEntry> _reportLogs = [];
    [ObservableProperty] private ObservableCollection<ShiftReportLogEntry> _filteredReportLogs = [];
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isLoading;

    public int TotalReports => ReportLogs.Count;
    public int NegativeDiscrepancyReports => ReportLogs.Count(log => log.Discrepancy < 0m);
    public decimal TotalRevenue => ReportLogs.Sum(log => log.TotalRevenue);

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    [RelayCommand]
    private async Task LoadLogsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = string.Empty;

            int? userId = _currentUserService.IsSale && _currentUserService.UserId.HasValue
                ? _currentUserService.UserId.Value
                : null;

            var logs = await _shiftService.GetClosedReportLogsAsync(userId);
            ReportLogs = new ObservableCollection<ShiftReportLogEntry>(logs);
            ApplyFilter();
            NotifyStatsChanged();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load shift report logs: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFilter()
    {
        var keyword = SearchText.Trim();
        IEnumerable<ShiftReportLogEntry> filtered = ReportLogs;

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            filtered = filtered.Where(log =>
                log.StaffEmail.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                log.ShiftId.ToString().Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                (log.Notes?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        FilteredReportLogs = new ObservableCollection<ShiftReportLogEntry>(filtered);
    }

    private void NotifyStatsChanged()
    {
        OnPropertyChanged(nameof(TotalReports));
        OnPropertyChanged(nameof(NegativeDiscrepancyReports));
        OnPropertyChanged(nameof(TotalRevenue));
    }
}
