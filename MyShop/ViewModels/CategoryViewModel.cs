using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class CategoryViewModel : ObservableObject
{
    private readonly CategoryService _service;
    private readonly SettingsManager _settingsManager;

    public record AddEditCategoryPayload(string Name, string? Description);

    public Func<Category?, Task<AddEditCategoryPayload?>>? ShowAddEditCategoryFormAsync { get; set; }

    public Func<string, string, Task<bool>>? ShowConfirmationDialogAsync { get; set; }

    public CategoryViewModel(CategoryService service, SettingsManager settingsManager)
    {
        _service = service;
        _settingsManager = settingsManager;
        var savedPageSize = _settingsManager.GetItemsPerPage();
        PageSize = Math.Max(1, savedPageSize);
        _ = LoadCategoriesAsync();
    }

    [ObservableProperty]
    private ObservableCollection<Category> _categories = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _successMessage = string.Empty;

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

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    partial void OnSearchQueryChanged(string value)
    {
        CurrentPage = 1;
        _ = LoadCategoriesAsync();
    }

    public bool ShowEmptyState => !IsLoading && Categories.Count == 0;

    [RelayCommand]
    public async Task LoadCategoriesAsync()
    {
        PageSize = Math.Max(1, _settingsManager.GetItemsPerPage());
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            var (items, totalCount) = await _service.GetCategoriesAsync(
                CurrentPage,
                PageSize,
                SearchQuery);

            Categories = new ObservableCollection<Category>(items);
            TotalItems = totalCount;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
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
        await LoadCategoriesAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadCategoriesAsync();
        }
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadCategoriesAsync();
        }
    }

    [RelayCommand]
    private async Task AddCategory()
    {
        if (ShowAddEditCategoryFormAsync is null) return;

        var payload = await ShowAddEditCategoryFormAsync(null);
        if (payload is null) return;

        bool success = await ExecuteMutationAsync(async () =>
        {
            var category = new Category
            {
                Name = payload.Name,
                Description = payload.Description
            };
            await _service.AddAsync(category);
        });

        if (success) SuccessMessage = $"Category \"{payload.Name}\" has been added";
    }

    [RelayCommand]
    private async Task EditCategory(Category? category)
    {
        if (category is null || ShowAddEditCategoryFormAsync is null) return;

        var payload = await ShowAddEditCategoryFormAsync(category);
        if (payload is null) return;

        bool success = await ExecuteMutationAsync(async () =>
        {
            var updatedCategory = new Category
            {
                Id = category.Id,
                Name = payload.Name,
                Description = payload.Description
            };
            await _service.UpdateAsync(updatedCategory);
        });

        if (success) SuccessMessage = $"Category \"{payload.Name}\" has been updated";
    }

    [RelayCommand]
    private async Task DeleteCategory(Category? category)
    {
        if (category is null || ShowConfirmationDialogAsync is null) return;

        bool confirmed = await ShowConfirmationDialogAsync(
            "Confirm deletion",
            $"Are you sure you want to delete category \"{category.Name}\"?\nThis action cannot be undone."
        );

        if (!confirmed) return;

        bool success = await ExecuteMutationAsync(async () => await _service.DeleteAsync(category.Id));
        if (success) SuccessMessage = $"Category \"{category.Name}\" has been deleted";
    }

    private async Task<bool> ExecuteMutationAsync(Func<Task> mutation)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            await mutation();
            await LoadCategoriesAsync();
            return true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnIsLoadingChanged(bool value) => OnPropertyChanged(nameof(ShowEmptyState));
    partial void OnCategoriesChanged(ObservableCollection<Category> value) => OnPropertyChanged(nameof(ShowEmptyState));
}
