using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class SportItemDetailViewModel : ObservableObject
{
    private readonly SportItemService _service;
    private readonly CategoryService _catService;
    private readonly IAiService _aiService;
    private readonly IFilePickerService _filePickerService;
    private byte[]? _lastUploadedImageBytes;

    public Func<string, string, Task<bool>>? ShowConfirmationDialogAsync { get; set; }

    public SportItemDetailViewModel(SportItemService service, CategoryService catService, IAiService aiService, IFilePickerService filePickerService)
    {
        _service = service;
        _catService = catService;
        _aiService = aiService;
        _filePickerService = filePickerService;
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
                ProductDescriptionUi = Item.Description ?? string.Empty;
                
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
            Item.Description = ProductDescriptionUi;

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
        try
        {
            var path = await _filePickerService.PickOpenFileAsync("Image files", new[] { ".jpg", ".jpeg", ".png" });
            
            if (!string.IsNullOrEmpty(path))
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var bytes = await File.ReadAllBytesAsync(path);
                _lastUploadedImageBytes = bytes; // Cache the bytes for AI
                
                var publicUrl = await _service.UploadImageAsync(bytes, Path.GetFileName(path));
                ImageUrls.Add(publicUrl);
                SelectedImageIndex = ImageUrls.Count - 1;
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

    [RelayCommand]
    private async Task GenerateAiDescriptionAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine("You are an expert sports equipment copywriter.");
            sb.AppendLine("CRITICAL INSTRUCTIONS:");
            sb.AppendLine("1. Write a CONCISE and EFFECTIVE product description (max 2-3 short paragraphs).");
            sb.AppendLine("2. Focus on the product's unique value proposition and professional features.");
            sb.AppendLine("3. Do NOT include technical specification tables or excessive marketing fluff.");
            sb.AppendLine("4. Analyze the image to identify key visual materials and design elements.");
            sb.AppendLine("5. Provide ONLY the description text. No conversational filler.");
            sb.AppendLine("\nPRODUCT DATA:");
            sb.AppendLine($"- Name: {Item.Name}");
            if (SelectedCategory != null)
                sb.AppendLine($"- Category: {SelectedCategory.Name}");
            if (Item.SellingPrice.HasValue)
                sb.AppendLine($"- Price: ${Item.SellingPrice.Value}");
            
            if (Variants.Count > 0)
            {
                var sizes = Variants.Select(v => v.Size).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
                var colors = Variants.Select(v => v.Color).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();
                if (sizes.Count > 0) sb.AppendLine($"- Available Sizes: {string.Join(", ", sizes)}");
                if (colors.Count > 0) sb.AppendLine($"- Available Colors: {string.Join(", ", colors)}");
            }

            sb.AppendLine("\nWrite a concise, high-impact description now:");

            byte[]? imageBytes = _lastUploadedImageBytes;
            
            // If we don't have cached bytes, try to download from URL
            if (imageBytes == null && !string.IsNullOrEmpty(PreviewImageUrl))
            {
                try
                {
                    using var client = new HttpClient();
                    imageBytes = await client.GetByteArrayAsync(PreviewImageUrl);
                }
                catch { /* Ignore */ }
            }

            var description = await _aiService.GenerateDescriptionAsync(sb.ToString(), imageBytes);
            ProductDescriptionUi = description;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"AI Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AutoFillDetailsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            byte[]? imageBytes = _lastUploadedImageBytes;

            // If we don't have cached bytes, try to download from current preview URL
            if (imageBytes == null && !string.IsNullOrEmpty(PreviewImageUrl))
            {
                try
                {
                    ErrorMessage = "Downloading image for analysis...";
                    using var client = new HttpClient();
                    imageBytes = await client.GetByteArrayAsync(PreviewImageUrl);
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Failed to download image: {ex.Message}";
                    return;
                }
            }

            if (imageBytes == null)
            {
                ErrorMessage = "Please upload an image first to use AI Auto-fill.";
                return;
            }

            await AnalyzeAndFillAsync(imageBytes);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"AI Auto-fill Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task AnalyzeAndFillAsync(byte[] bytes)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = "AI is analyzing product details...";
            
            // Pass the list of available category names to the AI
            var categoryNames = Categories.Select(c => c.Name).ToArray();
            var json = await _aiService.AnalyzeItemAsync(bytes, categoryNames);
            System.Console.WriteLine($"[AI] Raw JSON: {json}");

            // Clean JSON from any markdown wrappers
            if (json.Contains("```"))
            {
                int start = json.IndexOf("{");
                int end = json.LastIndexOf("}");
                if (start >= 0 && end > start)
                {
                    json = json.Substring(start, end - start + 1);
                }
            }
            json = json.Trim();
            
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // 1. Name
            if (root.TryGetProperty("name", out var nameProp))
            {
                Item.Name = nameProp.GetString();
            }

            // 2. Price
            if (root.TryGetProperty("price", out var priceProp))
            {
                if (priceProp.ValueKind == JsonValueKind.Number)
                    SellingPriceText = priceProp.GetDouble().ToString(CultureInfo.InvariantCulture);
                else if (priceProp.ValueKind == JsonValueKind.String)
                    SellingPriceText = priceProp.GetString();
            }

            // 3. Cost Price
            if (root.TryGetProperty("cost_price", out var costProp))
            {
                if (costProp.ValueKind == JsonValueKind.Number)
                    CostPriceText = costProp.GetDouble().ToString(CultureInfo.InvariantCulture);
                else if (costProp.ValueKind == JsonValueKind.String)
                    CostPriceText = costProp.GetString();
            }

            // 4. Low Stock Threshold
            if (root.TryGetProperty("low_stock_threshold", out var lowStockProp))
            {
                if (lowStockProp.ValueKind == JsonValueKind.Number)
                    LowStockThresholdText = lowStockProp.GetInt32().ToString();
                else if (lowStockProp.ValueKind == JsonValueKind.String)
                    LowStockThresholdText = lowStockProp.GetString();
            }

            // 5. Description
            if (root.TryGetProperty("description", out var descProp))
            {
                ProductDescriptionUi = descProp.GetString() ?? string.Empty;
            }

            // 6. Category Matching
            if (root.TryGetProperty("category", out var catProp))
            {
                var catName = catProp.GetString();
                var found = Categories.FirstOrDefault(c => c.Name.Equals(catName, StringComparison.OrdinalIgnoreCase));
                if (found != null)
                {
                    SelectedCategory = found;
                }
            }

            // 7. Suggested Variants
            if (root.TryGetProperty("suggested_variants", out var variantsProp) && variantsProp.ValueKind == JsonValueKind.Array)
            {
                Variants.Clear();
                foreach (var vElement in variantsProp.EnumerateArray())
                {
                    var variant = new SportItemVariant { SportItemId = Item.Id };
                    if (vElement.TryGetProperty("size", out var s)) variant.Size = s.GetString();
                    if (vElement.TryGetProperty("color", out var c)) variant.Color = c.GetString();
                    if (vElement.TryGetProperty("sku", out var sk)) variant.Sku = sk.GetString();
                    variant.StockQuantity = 0; // Enforce 0 initial stock
                    Variants.Add(variant);
                }
            }
            else if (root.TryGetProperty("color", out var colorProp) && Variants.Count > 0)
            {
                // Fallback for single color
                Variants[0].Color = colorProp.GetString();
            }

            // Notify all property changes
            OnPropertyChanged(nameof(Item));
            OnPropertyChanged(nameof(SellingPriceText));
            OnPropertyChanged(nameof(CostPriceText));
            OnPropertyChanged(nameof(LowStockThresholdText));
            OnPropertyChanged(nameof(ProductDescriptionUi));
            OnPropertyChanged(nameof(SelectedCategory));
            OnPropertyChanged(nameof(Variants));

            ErrorMessage = string.Empty;
            System.Console.WriteLine("[AI] Auto-fill complete.");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[AI AutoFill] Error: {ex.Message}");
            ErrorMessage = string.Empty;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
