using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class CategoryViewModel : ObservableObject
{
    private readonly CategoryService _service;

    public record AddEditCategoryPayload(string Name, string? Description);

    public Func<Category?, Task<AddEditCategoryPayload?>>? ShowAddEditCategoryFormAsync { get; set; }

    public Func<string, string, Task<bool>>? ShowConfirmationDialogAsync { get; set; }

    public CategoryViewModel(CategoryService service)
    {
        _service = service;
        // Tự động tải danh sách khi khởi tạo
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

    public bool ShowEmptyState => !IsLoading && Categories.Count == 0;

    [RelayCommand]
    public async Task LoadCategoriesAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            var result = await _service.GetAllAsync();
            Categories = new ObservableCollection<Category>(result);
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
    private async Task AddCategory()
    {
        if (ShowAddEditCategoryFormAsync is null)
        {
            return;
        }

        var payload = await ShowAddEditCategoryFormAsync(null);
        if (payload is null)
        {
            return;
        }

        bool success = await ExecuteMutationAsync(async () =>
        {
            var category = new Category
            {
                Name = payload.Name,
                Description = payload.Description
            };

            await _service.AddAsync(category);
        });

        if (success)
        {
            SuccessMessage = $"Đã thêm danh mục \"{payload.Name}\"";
        }
    }

    [RelayCommand]
    private async Task EditCategory(Category? category)
    {
        if (category is null || ShowAddEditCategoryFormAsync is null)
        {
            return;
        }

        var payload = await ShowAddEditCategoryFormAsync(category);
        if (payload is null)
        {
            return;
        }

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

        if (success)
        {
            SuccessMessage = $"Đã sửa danh mục \"{payload.Name}\"";
        }
    }

    [RelayCommand]
    private async Task DeleteCategory(Category? category)
    {
        if (category is null || ShowConfirmationDialogAsync is null)
        {
            return;
        }

        bool confirmed = await ShowConfirmationDialogAsync(
            "Xác nhận xóa",
            $"Bạn có chắc chắn muốn xóa danh mục \"{category.Name}\"?\nQuyết định này không thể hoàn tác."
        );

        if (!confirmed)
        {
            return;
        }

        bool success = await ExecuteMutationAsync(async () => await _service.DeleteAsync(category.Id));
        if (success)
        {
            SuccessMessage = $"Đã xóa danh mục \"{category.Name}\"";
        }
    }

    private async Task<bool> ExecuteMutationAsync(Func<Task> mutation)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            await mutation();

            var result = await _service.GetAllAsync();
            Categories = new ObservableCollection<Category>(result);
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

    partial void OnIsLoadingChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowEmptyState));
    }

    partial void OnCategoriesChanged(ObservableCollection<Category> value)
    {
        OnPropertyChanged(nameof(ShowEmptyState));
    }
}
