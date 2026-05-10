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
    private readonly SettingsManager _settingsManager;

    public ShiftReportLogsViewModel(ShiftService shiftService, CurrentUserService currentUserService, SettingsManager settingsManager)
    {
        _shiftService = shiftService;
        _currentUserService = currentUserService;
        _settingsManager = settingsManager;

        _pageSize = _settingsManager.GetItemsPerPage();
    }

    [ObservableProperty] private ObservableCollection<ShiftReportLogEntry> _reportLogs = [];
    [ObservableProperty] private ObservableCollection<ShiftReportLogEntry> _pagedReportLogs = [];
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isLoading;

    // Pagination properties
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPages))]
    [NotifyPropertyChangedFor(nameof(DisplayFrom))]
    [NotifyPropertyChangedFor(nameof(DisplayTo))]
    private int _currentPage = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPages))]
    [NotifyPropertyChangedFor(nameof(DisplayFrom))]
    [NotifyPropertyChangedFor(nameof(DisplayTo))]
    private int _pageSize = 10;

    private List<ShiftReportLogEntry> _allFilteredItems = [];

    public int TotalReports => ReportLogs.Count;
    public int NegativeDiscrepancyReports => ReportLogs.Count(log => log.Discrepancy < 0m);
    public decimal TotalRevenue => ReportLogs.Sum(log => log.TotalRevenue);

    public int TotalPages => (_allFilteredItems.Count + PageSize - 1) / Math.Max(1, PageSize);
    public int DisplayFrom => _allFilteredItems.Count == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
    public int DisplayTo => Math.Min(CurrentPage * PageSize, _allFilteredItems.Count);
    public int TotalFilteredItems => _allFilteredItems.Count;

    partial void OnSearchTextChanged(string value)
    {
        CurrentPage = 1;
        ApplyFilter();
    }

    [RelayCommand]
    public async Task LoadLogsAsync()
    {
        PageSize = Math.Max(1, _settingsManager.GetItemsPerPage());
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

        _allFilteredItems = filtered.ToList();
        UpdatePagedItems();
    }

    private void UpdatePagedItems()
    {
        var paged = _allFilteredItems
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        PagedReportLogs = new ObservableCollection<ShiftReportLogEntry>(paged);
        
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(DisplayFrom));
        OnPropertyChanged(nameof(DisplayTo));
        OnPropertyChanged(nameof(TotalFilteredItems));
    }

    [RelayCommand]
    private void Search()
    {
        CurrentPage = 1;
        ApplyFilter();
    }

    [RelayCommand]
    public void GoToPage(int page)
    {
        if (page >= 1 && page <= TotalPages)
        {
            CurrentPage = page;
            UpdatePagedItems();
        }
    }

    private void NotifyStatsChanged()
    {
        OnPropertyChanged(nameof(TotalReports));
        OnPropertyChanged(nameof(NegativeDiscrepancyReports));
        OnPropertyChanged(nameof(TotalRevenue));
    }
}
