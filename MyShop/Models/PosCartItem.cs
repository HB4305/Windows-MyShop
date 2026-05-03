using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyShop.Models;

public sealed class PosCartItem : INotifyPropertyChanged
{
    private int _quantity = 1;

    public required SportItemListRow Product { get; init; }

    public int ItemId => Product.Item.Id;
    public string Name => Product.Item.Name;
    public string CategoryName => Product.CategoryName;
    public string? ImageUrl => Product.Item.ImageUrl;
    public decimal UnitPrice => Product.Item.SellingPrice ?? 0m;
    public int AvailableStock => Math.Max(0, Product.Item.EffectiveStockQuantity);

    public int Quantity
    {
        get => _quantity;
        set
        {
            var next = Math.Clamp(value, 1, Math.Max(1, AvailableStock));
            if (_quantity == next)
            {
                return;
            }

            _quantity = next;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LineTotal));
            OnPropertyChanged(nameof(QuantityDisplay));
        }
    }

    public decimal LineTotal => UnitPrice * Quantity;
    public string QuantityDisplay => Quantity.ToString();

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
