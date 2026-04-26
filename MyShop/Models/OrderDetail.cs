using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Linq;

namespace MyShop.Models;

public class OrderDetail : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public int Id { get; set; }

    public int? OrderId { get; set; }

    public int? ItemId { get; set; }

    [MaxLength(255, ErrorMessage = "Item name must not exceed 255 characters")]
    public string? ItemName { get; set; }

    private int _quantity;
    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity != value)
            {
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPriceDisplay));
                OnPropertyChanged(nameof(QuantityPriceDisplay));
                OnPropertyChanged(nameof(StockStatusDisplay));
            }
        }
    }

    [Required(ErrorMessage = "Unit price is required")]
    [Range(0, 9999999999.99, ErrorMessage = "Unit price exceeds allowed range")]
    public decimal UnitPrice { get; set; }

    public int LowStockThreshold { get; set; }

    public long? VariantId { get; set; }

    public string? Size { get; set; }
    public string? Color { get; set; }

    public List<SportItemVariant> AvailableVariants { get; set; } = new();

    public List<string> AvailableSizes => AvailableVariants
        .Select(v => v.Size ?? "N/A")
        .Distinct()
        .OrderBy(s => s)
        .ToList();

    public List<string> AvailableColors => AvailableVariants
        .Select(v => v.Color ?? "N/A")
        .Distinct()
        .OrderBy(c => c)
        .ToList();

    private string? _selectedSize;
    public string? SelectedSize
    {
        get => _selectedSize;
        set
        {
            if (_selectedSize != value)
            {
                _selectedSize = value;
                OnPropertyChanged();
                UpdateSelectedVariant();
            }
        }
    }

    private string? _selectedColor;
    public string? SelectedColor
    {
        get => _selectedColor;
        set
        {
            if (_selectedColor != value)
            {
                _selectedColor = value;
                OnPropertyChanged();
                UpdateSelectedVariant();
                OnPropertyChanged(nameof(StockStatusDisplay));
            }
        }
    }

    private void UpdateSelectedVariant()
    {
        var matched = AvailableVariants.FirstOrDefault(v =>
            (v.Size ?? "N/A") == (SelectedSize ?? "N/A") &&
            (v.Color ?? "N/A") == (SelectedColor ?? "N/A")
        );

        _selectedVariant = matched;
        if (matched != null)
        {
            VariantId = matched.Id;
            Size = matched.Size;
            Color = matched.Color;
        }
        else
        {
            VariantId = null;
            Size = null;
            Color = null;
        }
        OnPropertyChanged(nameof(StockStatusDisplay));
        OnPropertyChanged(nameof(MaxQuantity));
        OnPropertyChanged(nameof(IsQuantityEnabled));
    }

    private SportItemVariant? _selectedVariant;
    public SportItemVariant? SelectedVariant
    {
        get => _selectedVariant;
        set
        {
            if (_selectedVariant != value)
            {
                _selectedVariant = value;
                if (value != null)
                {
                    VariantId = value.Id;
                    _selectedSize = value.Size ?? "N/A";
                    _selectedColor = value.Color ?? "N/A";
                    Size = value.Size;
                    Color = value.Color;
                    OnPropertyChanged(nameof(SelectedSize));
                    OnPropertyChanged(nameof(SelectedColor));
                    OnPropertyChanged(nameof(AvailableColors));
                    OnPropertyChanged(nameof(StockStatusDisplay));
                    OnPropertyChanged(nameof(MaxQuantity));
                    OnPropertyChanged(nameof(IsQuantityEnabled));
                }
            }
        }
    }

    public string StockStatusDisplay
    {
        get
        {
            if (SelectedVariant == null)
            {
                if (!string.IsNullOrEmpty(SelectedSize) && !string.IsNullOrEmpty(SelectedColor))
                {
                    return "🔴 Out of stock"; // The combination does not exist
                }
                return "⚪ Select variant";
            }
            
            int stock = SelectedVariant.StockQuantity;
            if (stock <= 0) return "🔴 Out of stock";
            if (Quantity > stock) return $"❌ Max available: {stock}";

            int threshold = LowStockThreshold > 0 ? LowStockThreshold : 5;
            if (stock <= threshold) return $"⚠️ Only {stock} left!";
            return $"🟢 In stock: {stock}";
        }
    }

    public string TotalPriceDisplay => (Quantity * UnitPrice).ToString("C");

    public string UnitPriceDisplay => UnitPrice.ToString("C");

    public string QuantityPriceDisplay => $"{Quantity} x {UnitPriceDisplay}";

    public double MaxQuantity
    {
        get
        {
            if (SelectedVariant == null) return 999;
            return SelectedVariant.StockQuantity > 0 ? SelectedVariant.StockQuantity : 1;
        }
    }

    public bool IsQuantityEnabled
    {
        get
        {
            if (SelectedVariant == null) return true;
            return SelectedVariant.StockQuantity > 0;
        }
    }
}
