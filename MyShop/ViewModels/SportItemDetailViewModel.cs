using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class SportItemDetailViewModel : ObservableObject
{
    private readonly SportItemService _service;
    private readonly CategoryService _catService;

    public Func<string, string, Task<bool>>? ShowConfirmationDialogAsync { get; set; }

    public SportItemDetailViewModel(SportItemService service, CategoryService catService)
    {
        _service = service;
        _catService = catService;
        ImageUrls.CollectionChanged += OnImageUrlsCollectionChanged;
    }

    private void OnImageUrlsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Remove:
                if (e.OldStartingIndex >= 0 && e.OldStartingIndex < SelectedImageIndex)
                    SelectedImageIndex--;
                else if (e.OldStartingIndex >= 0
                         && e.OldStartingIndex == SelectedImageIndex
                         && SelectedImageIndex >= ImageUrls.Count)
                    SelectedImageIndex = Math.Max(0, ImageUrls.Count - 1);
                break;
            case NotifyCollectionChangedAction.Reset:
                SelectedImageIndex = 0;
                break;
            case NotifyCollectionChangedAction.Move:
                AdjustSelectedIndexAfterMove(e.OldStartingIndex, e.NewStartingIndex);
                break;
        }

        if (ImageUrls.Count == 0)
            SelectedImageIndex = 0;
        else if (SelectedImageIndex >= ImageUrls.Count)
            SelectedImageIndex = ImageUrls.Count - 1;

        OnPropertyChanged(nameof(PreviewImageUrl));
    }

    /// <summary>Keeps selection aligned when items reorder (drag-drop or Move).</summary>
    private void AdjustSelectedIndexAfterMove(int oldIdx, int newIdx)
    {
        if (oldIdx < 0 || newIdx < 0) return;
        if (SelectedImageIndex == oldIdx)
            SelectedImageIndex = newIdx;
        else if (oldIdx < newIdx && oldIdx < SelectedImageIndex && SelectedImageIndex <= newIdx)
            SelectedImageIndex--;
        else if (oldIdx > newIdx && newIdx <= SelectedImageIndex && SelectedImageIndex < oldIdx)
            SelectedImageIndex++;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditMode))]
    private SportItem _item = new();

    public bool IsEditMode => Item.Id != 0;

    partial void OnItemChanged(SportItem value)
    {
        OnPropertyChanged(nameof(SellingPriceText));
        OnPropertyChanged(nameof(CostPriceText));
        OnPropertyChanged(nameof(LowStockThresholdText));
    }

    // Wrapper properties for TextBox binding (string ↔ model decimal?/int?)
    public string SellingPriceText
    {
        get => Item.SellingPrice.HasValue ? Item.SellingPrice.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
        set
        {
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                Item.SellingPrice = d;
            else if (string.IsNullOrWhiteSpace(value))
                Item.SellingPrice = null;
            OnPropertyChanged();
        }
    }

    public string CostPriceText
    {
        get => Item.CostPrice.HasValue ? Item.CostPrice.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
        set
        {
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                Item.CostPrice = d;
            else if (string.IsNullOrWhiteSpace(value))
                Item.CostPrice = null;
            OnPropertyChanged();
        }
    }

    public string LowStockThresholdText
    {
        get => Item.LowStockThreshold.HasValue ? Item.LowStockThreshold.Value.ToString() : string.Empty;
        set
        {
            if (int.TryParse(value, out var i))
                Item.LowStockThreshold = i;
            else if (string.IsNullOrWhiteSpace(value))
                Item.LowStockThreshold = null;
            OnPropertyChanged();
        }
    }

    [ObservableProperty]
    private ObservableCollection<Category> _categories = [];

    [ObservableProperty]
    private Category? _selectedCategory;

    [ObservableProperty]
    private ObservableCollection<string> _imageUrls = [];

    [ObservableProperty]
    private ObservableCollection<SportItemVariant> _variants = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewImageUrl))]
    private int _selectedImageIndex;

    /// <summary>URL of the image shown in the main preview (left).</summary>
    public string? PreviewImageUrl =>
        ImageUrls.Count > 0 && SelectedImageIndex >= 0 && SelectedImageIndex < ImageUrls.Count
            ? ImageUrls[SelectedImageIndex]
            : null;

    /// <summary>Description field on the form (no DB column yet).</summary>
    [ObservableProperty]
    private string _productDescriptionUi = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public async Task InitializeAsync(SportItem? item = null)
    {
        try
        {
            IsLoading = true;
            // Load categories for dropdown
            var cats = await _catService.GetAllAsync();
            Categories = new ObservableCollection<Category>(cats);

            if (item != null)
            {
                Item = item;
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == Item.CategoryId);
                
                // Load images for existing product
                ImageUrls.Clear();
                if (Item.ImageUrls != null)
                {
                    foreach (var url in Item.ImageUrls)
                    {
                        ImageUrls.Add(url);
                    }
                }

                Variants.Clear();
                foreach (var variant in Item.Variants)
                {
                    Variants.Add(new SportItemVariant
                    {
                        Id = variant.Id,
                        SportItemId = variant.SportItemId,
                        Size = variant.Size,
                        Color = variant.Color,
                        StockQuantity = variant.StockQuantity,
                        Sku = variant.Sku
                    });
                }

                if (Variants.Count == 0)
                    Variants.Add(new SportItemVariant { SportItemId = Item.Id });
            }
            else
            {
                Item = new SportItem();
                ImageUrls.Clear();
                ProductDescriptionUi = string.Empty;
                Variants =
                [
                    new SportItemVariant()
                ];
            }

            SelectedImageIndex = ImageUrls.Count > 0 ? 0 : 0;
            OnPropertyChanged(nameof(PreviewImageUrl));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Initialization error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public event Action? SaveCompleted;

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            IsLoading = true;
            if (SelectedCategory != null) Item.CategoryId = SelectedCategory.Id;

            NormalizeVariants();
            if (Variants.Count == 0)
            {
                ErrorMessage = "Please add at least one variant row with size or color before saving.";
                return;
            }

            Item.Variants = Variants.ToList();
            SyncLegacyFieldsFromVariants();
            Item.ImageUrls = ImageUrls.ToList();

            if (Item.Id == 0) 
            {
                var newId = await _service.AddAsync(Item);
                Item.Id = newId;
            }
            else 
            {
                await _service.UpdateAsync(Item);
            }
            
            SaveCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Save error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (Item.Id == 0 || ShowConfirmationDialogAsync is null)
        {
            return;
        }

        bool confirmed = await ShowConfirmationDialogAsync(
            "Confirm delete",
            $"Are you sure you want to delete \"{Item.Name}\"?\nThis action cannot be undone.");

        if (!confirmed)
        {
            return;
        }

        try
        {
            IsLoading = true;
            await _service.DeleteAsync(Item.Id);
            SaveCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Delete error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task PickImageAsync()
    {
#if WINDOWS
        try
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                using var stream = await file.OpenStreamForReadAsync();
                var buffer = new byte[stream.Length];
                await stream.ReadExactlyAsync(buffer);

                var publicUrl = await _service.UploadImageAsync(buffer, file.Name);
                ImageUrls.Add(publicUrl);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Image upload error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
#else
        ErrorMessage = "Image upload is only available on Windows.";
#endif
    }

    [RelayCommand]
    private void DeleteImage(string? url)
    {
        if (string.IsNullOrEmpty(url)) return;
        ImageUrls.Remove(url);
    }

    [RelayCommand]
    private void MoveSelectedImageEarlier()
    {
        if (SelectedImageIndex <= 0 || SelectedImageIndex >= ImageUrls.Count) return;
        ImageUrls.Move(SelectedImageIndex, SelectedImageIndex - 1);
    }

    [RelayCommand]
    private void MoveSelectedImageLater()
    {
        if (SelectedImageIndex < 0 || SelectedImageIndex >= ImageUrls.Count - 1) return;
        ImageUrls.Move(SelectedImageIndex, SelectedImageIndex + 1);
    }

    [RelayCommand]
    private void AddVariant()
    {
        Variants.Add(new SportItemVariant
        {
            SportItemId = Item.Id
        });
    }

    [RelayCommand]
    private void RemoveVariant(SportItemVariant? variant)
    {
        if (variant is null)
            return;

        if (Variants.Count <= 1)
        {
            variant.Size = null;
            variant.Color = null;
            variant.StockQuantity = 0;
            variant.Sku = null;
            return;
        }

        Variants.Remove(variant);
    }

    private void NormalizeVariants()
    {
        foreach (var variant in Variants)
        {
            variant.Size = string.IsNullOrWhiteSpace(variant.Size) ? null : variant.Size.Trim();
            variant.Color = string.IsNullOrWhiteSpace(variant.Color) ? null : variant.Color.Trim();
            variant.Sku = string.IsNullOrWhiteSpace(variant.Sku) ? null : variant.Sku.Trim();
            // Ensure StockQuantity is non-negative
            if (variant.StockQuantity < 0)
                variant.StockQuantity = 0;
        }

        var cleaned = Variants
            .Where(v => !string.IsNullOrWhiteSpace(v.Size)
                        || !string.IsNullOrWhiteSpace(v.Color)
                        || !string.IsNullOrWhiteSpace(v.Sku)
                        || v.StockQuantity > 0)
            .ToList();

        Variants = new ObservableCollection<SportItemVariant>(cleaned);
    }

    private void SyncLegacyFieldsFromVariants()
    {
        Item.StockQuantity = Variants.Sum(v => Math.Max(0, v.StockQuantity));
    }
}
