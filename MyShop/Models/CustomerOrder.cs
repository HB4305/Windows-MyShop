using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace MyShop.Models;

public class CustomerOrder : INotifyPropertyChanged
{
    public int Id { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public int? CustomerId { get; set; }

    private string _customerName = string.Empty;
    [Required(ErrorMessage = "Customer name is required")]
    public string CustomerName
    {
        get => _customerName;
        set => SetProperty(ref _customerName, value);
    }

    private string _customerPhone = string.Empty;
    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string CustomerPhone
    {
        get => _customerPhone;
        set => SetProperty(ref _customerPhone, value);
    }

    private string? _shippingAddress;
    public string? ShippingAddress
    {
        get => _shippingAddress;
        set => SetProperty(ref _shippingAddress, value);
    }

    private string? _orderType;
    [RegularExpression("^(AtStore|Delivery)$", ErrorMessage = "Order type must be 'AtStore' or 'Delivery'")]
    public string? OrderType
    {
        get => _orderType;
        set => SetProperty(ref _orderType, value);
    }

    private string? _status = "Pending";
    public string? Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    private string? _paymentStatus = "Unpaid";
    public string? PaymentStatus
    {
        get => _paymentStatus;
        set => SetProperty(ref _paymentStatus, value);
    }

    private decimal? _totalAmount = 0M;
    [Range(0, 9999999999.99, ErrorMessage = "Total amount must be positive")]
    public decimal? TotalAmount
    {
        get => _totalAmount;
        set => SetProperty(ref _totalAmount, value);
    }

    private string? _notes = string.Empty;
    public string? Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    // Who created this order (FK -> users.id)
    public int? SellerId { get; set; }

    // Seller name (denormalized, for fast display)
    public string? SellerName { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(storage, value)) return false;
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
