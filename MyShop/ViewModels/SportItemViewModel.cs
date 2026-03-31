using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class SportItemViewModel : ObservableObject
{
    private readonly SportItemService _service;

    public SportItemViewModel(SportItemService service)
    {
        _service = service;
    }

    [ObservableProperty]
    private ObservableCollection<SportItem> _items = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    // Phân trang
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

    // Lọc và Tìm kiếm
    [ObservableProperty]
    private string _searchKeyword = string.Empty;

    [ObservableProperty]
    private double? _minPrice;

    [ObservableProperty]
    private double? _maxPrice;

    // Sắp xếp
    [ObservableProperty]
    private string _sortField = "name";

    [ObservableProperty]
    private bool _isSortAscending = true;

    [RelayCommand]
    public async Task LoadItemsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            var (items, totalCount) = await _service.GetItemsAsync(
                CurrentPage, 
                PageSize, 
                SearchKeyword, 
                (decimal?)MinPrice, 
                (decimal?)MaxPrice, 
                SortField, 
                IsSortAscending);

            Items = new ObservableCollection<SportItem>(items);
            TotalItems = totalCount;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi: {ex.Message}";
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
