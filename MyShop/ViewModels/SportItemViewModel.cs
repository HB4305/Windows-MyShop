using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class SportItemViewModel : ObservableObject
{
    public const double PriceUsdSliderMax = 500;

    private readonly SportItemService _service;
    private readonly CategoryService _categoryService;
    private readonly SettingsManager _settingsManager;
    private int _priceFilterDebounce;

    public SportItemViewModel(SportItemService service, CategoryService categoryService, SettingsManager settingsManager)
    {
        _service = service;
        _categoryService = categoryService;
        _settingsManager = settingsManager;
        var savedPageSize = _settingsManager.GetItemsPerPage();
        PageSize = Math.Max(1, savedPageSize);
    }

    [ObservableProperty]
    private ObservableCollection<SportItemListRow> _items = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    // Pagination
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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPages))]
    [NotifyPropertyChangedFor(nameof(DisplayFrom))]
    [NotifyPropertyChangedFor(nameof(DisplayTo))]
    private int _totalItems;

    public int TotalPages => (TotalItems + PageSize - 1) / Math.Max(1, PageSize);
    public int DisplayFrom => TotalItems == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
    public int DisplayTo => Math.Min(CurrentPage * PageSize, TotalItems);

    // Filtering and Searching
    [ObservableProperty]
    private string _searchKeyword = string.Empty;

    [ObservableProperty]
    private double? _minPrice;

    [ObservableProperty]
    private double? _maxPrice;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PriceRangeSummary))]
    private double _priceUsdMin;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PriceRangeSummary))]
    private double _priceUsdMax = PriceUsdSliderMax;

    /// <summary>Filtered price range summary (display only; synchronized with range sliders).</summary>
    public string PriceRangeSummary
    {
        get
        {
            var lo = PriceUsdMin.ToString("N2", CultureInfo.InvariantCulture);
            if (PriceUsdMax >= PriceUsdSliderMax - 0.0001)
                return $"${lo} – ${PriceUsdSliderMax.ToString("N2", CultureInfo.InvariantCulture)}+";
            var hi = PriceUsdMax.ToString("N2", CultureInfo.InvariantCulture);
            return $"${lo} – ${hi}";
        }
    }

    // Sorting
    [ObservableProperty]
    private string _sortField = "name";

    [ObservableProperty]
    private bool _isSortAscending = true;

    /// <summary>Called after the UI sort field (ComboBox) changes to reload the list.</summary>
    public Task ReloadWithCurrentSortAsync()
    {
        CurrentPage = 1;
        return LoadItemsAsync();
    }

    partial void OnIsSortAscendingChanged(bool value)
    {
        if (string.IsNullOrWhiteSpace(SortField))
            return;
        CurrentPage = 1;
        _ = LoadItemsAsync();
    }

    partial void OnPriceUsdMinChanged(double value)
    {
        if (value > PriceUsdMax)
            PriceUsdMax = value;
        SyncPriceFiltersFromSliders();
        SchedulePriceFilterSearch();
    }

    partial void OnPriceUsdMaxChanged(double value)
    {
        if (value < PriceUsdMin)
            PriceUsdMin = value;
        SyncPriceFiltersFromSliders();
        SchedulePriceFilterSearch();
    }

    private void SyncPriceFiltersFromSliders()
    {
        if (PriceUsdMin <= 0)
            MinPrice = null;
        else
            MinPrice = PriceUsdMin;

        if (PriceUsdMax >= PriceUsdSliderMax - 0.0001)
            MaxPrice = null;
        else
            MaxPrice = PriceUsdMax;
    }

    private void SchedulePriceFilterSearch()
    {
        var gen = ++_priceFilterDebounce;
        _ = DebounceSearchAsync(gen);
    }

    private async Task DebounceSearchAsync(int gen)
    {
        await Task.Delay(280);
        if (gen != _priceFilterDebounce)
            return;

        var dq = DispatcherQueue.GetForCurrentThread();
        if (dq != null)
        {
            dq.TryEnqueue(() => SearchCommand.Execute(null));
        }
        else
        {
            SearchCommand.Execute(null);
        }
    }

    [RelayCommand]
    public async Task LoadItemsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var (rows, totalCount) = await _service.GetItemsAsync(
                CurrentPage,
                PageSize,
                SearchKeyword,
                (decimal?)MinPrice,
                (decimal?)MaxPrice,
                SortField,
                IsSortAscending);

            Items = new ObservableCollection<SportItemListRow>(rows);
            TotalItems = totalCount;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadItemsAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage * PageSize < TotalItems)
        {
            CurrentPage++;
            await LoadItemsAsync();
        }
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadItemsAsync();
        }
    }
}
