using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Models.Ai;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class SportItemDetailViewModel : ObservableObject
{
    private readonly SportItemService _service;
    private readonly CategoryService _catService;
    private readonly IAiService _aiService;
    private readonly IFilePickerService _filePickerService;
    private readonly AiLogService _aiLogService;
    private byte[]? _lastUploadedImageBytes;
    private AiItemAnalysis? _pendingAiAnalysis;

    public Func<string, string, Task<bool>>? ShowConfirmationDialogAsync { get; set; }

    public SportItemDetailViewModel(SportItemService service, CategoryService catService, IAiService aiService, IFilePickerService filePickerService, AiLogService aiLogService)
    {
        _service = service;
        _catService = catService;
        _aiService = aiService;
        _filePickerService = filePickerService;
        _aiLogService = aiLogService;
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
    private ObservableCollection<AiFieldSuggestion> _aiSuggestions = [];

    [ObservableProperty]
    private bool _isAiReviewVisible;

    [ObservableProperty]
    private string _aiReviewSummary = string.Empty;

    [ObservableProperty]
    private double _aiOverallConfidence;

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
    private void ViewImageDetail(string? url)
    {
        if (string.IsNullOrEmpty(url)) url = PreviewImageUrl;
        if (string.IsNullOrEmpty(url)) return;
        ViewImageDetailRequested?.Invoke(this, url);
    }

    public event EventHandler<string>? ViewImageDetailRequested;

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

            var categoryHint = BuildCategoryHint(SelectedCategory);
            if (!string.IsNullOrWhiteSpace(categoryHint))
            {
                sb.AppendLine($"- Category preset: {categoryHint}");
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
        var sw = Stopwatch.StartNew();
        var logEntry = new AiLogEntry
        {
            Operation = "ai_autofill"
        };

        try
        {
            IsLoading = true;
            ErrorMessage = "AI is analyzing product details...";
            
            var categoryNames = Categories.Select(c => c.Name).ToArray();
            var categoryHint = BuildCategoryHint(SelectedCategory);
            var rawJson = await _aiService.AnalyzeItemAsync(bytes, categoryNames, categoryHint);

            var cleanedJson = CleanJsonPayload(rawJson);
            var analysis = ParseAiAnalysis(cleanedJson);
            if (analysis == null)
            {
                throw new InvalidOperationException("AI response is not valid JSON.");
            }

            var validationErrors = ValidateAiAnalysis(analysis, categoryNames);
            if (validationErrors.Count > 0)
            {
                var repairedJson = await _aiService.RepairItemJsonAsync(cleanedJson, validationErrors.ToArray(), categoryNames);
                repairedJson = CleanJsonPayload(repairedJson);
                var repaired = ParseAiAnalysis(repairedJson);
                if (repaired != null)
                {
                    analysis = repaired;
                }
                else
                {
                    throw new InvalidOperationException("AI repair returned invalid JSON.");
                }
            }

            _pendingAiAnalysis = analysis;
            BuildAiSuggestions(analysis);
            AiOverallConfidence = analysis.Confidence ?? 0;
            
            // Auto-apply all suggestions immediately
            ApplyAiSuggestions();

            ErrorMessage = string.Empty;

            logEntry.Success = true;
            logEntry.ResponseSummary = $"fields={AiSuggestions.Count};confidence={AiOverallConfidence:0.00};errors={validationErrors.Count}";
            ErrorMessage = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"AI Auto-fill Error: {ex.Message}";
            logEntry.Success = false;
            logEntry.Error = ex.Message;
        }
        finally
        {
            sw.Stop();
            logEntry.DurationMs = sw.ElapsedMilliseconds;
            await SafeLogAsync(logEntry);
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ApplyAiSuggestions()
    {
        if (_pendingAiAnalysis == null || AiSuggestions.Count == 0)
        {
            return;
        }

        // Applying all suggestions because the user wants "enough fields" fulfilled.
        foreach (var suggestion in AiSuggestions)
        {
            switch (suggestion.Key)
            {
                case "name":
                    Item.Name = _pendingAiAnalysis.Name ?? Item.Name;
                    break;
                case "price":
                    if (_pendingAiAnalysis.Price.HasValue)
                        SellingPriceText = _pendingAiAnalysis.Price.Value.ToString(CultureInfo.InvariantCulture);
                    break;
                case "cost_price":
                    if (_pendingAiAnalysis.CostPrice.HasValue)
                        CostPriceText = _pendingAiAnalysis.CostPrice.Value.ToString(CultureInfo.InvariantCulture);
                    break;
                case "low_stock_threshold":
                    if (_pendingAiAnalysis.LowStockThreshold.HasValue)
                        LowStockThresholdText = _pendingAiAnalysis.LowStockThreshold.Value.ToString();
                    break;
                case "description":
                    if (!string.IsNullOrWhiteSpace(_pendingAiAnalysis.Description))
                        ProductDescriptionUi = _pendingAiAnalysis.Description;
                    break;
                case "color":
                    if (!string.IsNullOrWhiteSpace(_pendingAiAnalysis.Color) && Variants.Count > 0)
                        Variants[0].Color = _pendingAiAnalysis.Color;
                    break;
                case "category":
                    if (!string.IsNullOrWhiteSpace(_pendingAiAnalysis.Category))
                    {
                        var found = Categories.FirstOrDefault(c => c.Name.Equals(_pendingAiAnalysis.Category, StringComparison.OrdinalIgnoreCase));
                        if (found != null) SelectedCategory = found;
                    }
                    break;
                case "suggested_variants":
                    if (_pendingAiAnalysis.SuggestedVariants.Count > 0)
                    {
                        Variants.Clear();
                        foreach (var v in _pendingAiAnalysis.SuggestedVariants)
                        {
                            Variants.Add(new SportItemVariant
                            {
                                SportItemId = Item.Id,
                                Size = v.Size,
                                Color = v.Color,
                                Sku = v.Sku,
                                StockQuantity = 0
                            });
                        }
                    }
                    break;
            }
        }

        IsAiReviewVisible = false;
    }

    [RelayCommand]
    private void DismissAiSuggestions()
    {
        AiSuggestions.Clear();
        IsAiReviewVisible = false;
        AiReviewSummary = string.Empty;
        _pendingAiAnalysis = null;
    }

    private static string CleanJsonPayload(string json)
    {
        if (json.Contains("```"))
        {
            int start = json.IndexOf("{");
            int end = json.LastIndexOf("}");
            if (start >= 0 && end > start)
            {
                json = json.Substring(start, end - start + 1);
            }
        }
        return json.Trim();
    }

    private static AiItemAnalysis? ParseAiAnalysis(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<AiItemAnalysis>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }

    private static List<string> ValidateAiAnalysis(AiItemAnalysis analysis, string[] availableCategories)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(analysis.Name))
            errors.Add("Name is required.");

        if (!analysis.Price.HasValue || analysis.Price <= 0 || analysis.Price > 100000)
            errors.Add("Price must be a positive number under 100000.");

        if (analysis.CostPrice.HasValue && analysis.Price.HasValue && analysis.CostPrice > analysis.Price)
            errors.Add("Cost price must not exceed selling price.");

        if (analysis.LowStockThreshold.HasValue && (analysis.LowStockThreshold <= 0 || analysis.LowStockThreshold > 500))
            errors.Add("Low stock threshold must be between 1 and 500.");

        if (!string.IsNullOrWhiteSpace(analysis.Category))
        {
            var match = availableCategories.Any(c => c.Equals(analysis.Category, StringComparison.OrdinalIgnoreCase));
            if (!match) errors.Add("Category must be one of the available categories.");
        }
        else
        {
            errors.Add("Category is required.");
        }

        if (analysis.SuggestedVariants == null || analysis.SuggestedVariants.Count == 0)
            errors.Add("At least one suggested variant is required.");
        else
        {
            foreach (var v in analysis.SuggestedVariants)
            {
                if (string.IsNullOrWhiteSpace(v.Size) && string.IsNullOrWhiteSpace(v.Color))
                {
                    errors.Add("Each variant should include a size or color.");
                    break;
                }
            }
        }

        return errors;
    }

    private void BuildAiSuggestions(AiItemAnalysis analysis)
    {
        AiSuggestions.Clear();

        AddSuggestion("name", "Name", analysis.Name, analysis);
        AddSuggestion("price", "Price", analysis.Price?.ToString(CultureInfo.InvariantCulture), analysis);
        AddSuggestion("cost_price", "Cost price", analysis.CostPrice?.ToString(CultureInfo.InvariantCulture), analysis);
        AddSuggestion("category", "Category", analysis.Category, analysis);
        AddSuggestion("low_stock_threshold", "Low stock", analysis.LowStockThreshold?.ToString(), analysis);
        AddSuggestion("color", "Color", analysis.Color, analysis);
        AddSuggestion("description", "Description", analysis.Description, analysis);

        if (analysis.SuggestedVariants.Count > 0)
        {
            var summary = string.Join(", ", analysis.SuggestedVariants
                .Select(v => $"{v.Size}/{v.Color}")
                .Where(s => s != "/"));
            AddSuggestion("suggested_variants", "Variants", summary, analysis);
        }
    }

    private void AddSuggestion(string key, string label, string? value, AiItemAnalysis analysis)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        var confidence = analysis.FieldConfidence.TryGetValue(key, out var c) ? c : (analysis.Confidence ?? 0.5);
        var reason = analysis.FieldReasons.TryGetValue(key, out var r) ? r : string.Empty;

        AiSuggestions.Add(new AiFieldSuggestion
        {
            Key = key,
            Label = label,
            Value = value.Trim(),
            Confidence = confidence,
            Reason = reason,
            IsAccepted = confidence >= 0.4 // Lowered threshold to include more fields as requested
        });
    }

    private static string BuildCategoryHint(Category? category)
    {
        if (category == null || string.IsNullOrWhiteSpace(category.Name))
            return string.Empty;

        var name = category.Name.ToLowerInvariant();

        if (name.Contains("shoe") || name.Contains("footwear") || name.Contains("sneaker"))
            return "Use footwear sizing (US/EU), emphasize cushioning and traction.";

        if (name.Contains("apparel") || name.Contains("clothing") || name.Contains("jersey") || name.Contains("shirt"))
            return "Use apparel sizing (S/M/L/XL) and highlight fabric, fit, and breathability.";

        if (name.Contains("racket"))
            return "Use grip sizing (G2/G3 or 4 1/4) and highlight frame material and balance.";

        if (name.Contains("ball"))
            return "Use official ball size (e.g., Size 5/7) and note surface texture and bounce.";

        return "Focus on category-appropriate features and sizing.";
    }

    private async Task SafeLogAsync(AiLogEntry entry)
    {
        try
        {
            await _aiLogService.LogAsync(entry);
        }
        catch
        {
            // Intentionally ignore logging errors.
        }
    }
}
