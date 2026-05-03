using System.Collections.ObjectModel;
using System.Collections.Specialized;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class PosViewModel : ObservableObject
{
    private const decimal TaxRate = 0.085m;

    private readonly SportItemService _sportItemService;
    private readonly CategoryService _categoryService;
    private readonly CustomerService _customerService;
    private readonly CustomerOrderService _orderService;
    private readonly CurrentUserService _currentUserService;
    private readonly SettingsManager _settingsManager;
    private readonly Dictionary<string, int> _categoryIdsByName = new(StringComparer.OrdinalIgnoreCase);
    private List<string> _availableProductOptions = [];
    private bool _isApplyingCustomer;

    public PosViewModel(
        SportItemService sportItemService,
        CategoryService categoryService,
        CustomerService customerService,
        CustomerOrderService orderService,
        CurrentUserService currentUserService,
        SettingsManager settingsManager)
    {
        _sportItemService = sportItemService;
        _categoryService = categoryService;
        _customerService = customerService;
        _orderService = orderService;
        _currentUserService = currentUserService;
        _settingsManager = settingsManager;

        PageSize = Math.Max(1, _settingsManager.GetItemsPerPage());
        CategoryOptions = ["All Gear"];
        ProductOptions = [];
        CustomerSuggestions = [];
        Products = [];
        CartItems.CollectionChanged += CartItems_CollectionChanged;

        _ = InitializeAsync();
    }

    [ObservableProperty] private ObservableCollection<SportItemListRow> _products;
    [ObservableProperty] private ObservableCollection<PosCartItem> _cartItems = [];
    [ObservableProperty] private List<string> _categoryOptions;
    [ObservableProperty] private List<string> _productOptions;
    [ObservableProperty] private ObservableCollection<Customer> _customerSuggestions;
    [ObservableProperty] private Customer? _selectedCustomer;
    [ObservableProperty] private string _selectedCategory = "All Gear";
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _customerSearchText = string.Empty;
    [ObservableProperty] private string _customerName = "Walk-in Customer";
    [ObservableProperty] private string _customerPhone = "0000000000";
    [ObservableProperty] private string _customerAddress = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isCheckingOut;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPages))]
    [NotifyPropertyChangedFor(nameof(DisplayFrom))]
    [NotifyPropertyChangedFor(nameof(DisplayTo))]
    private int _currentPage = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPages))]
    [NotifyPropertyChangedFor(nameof(DisplayFrom))]
    [NotifyPropertyChangedFor(nameof(DisplayTo))]
    private int _pageSize;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPages))]
    [NotifyPropertyChangedFor(nameof(DisplayFrom))]
    [NotifyPropertyChangedFor(nameof(DisplayTo))]
    private int _totalItems;

    public int TotalPages => Math.Max(1, (TotalItems + PageSize - 1) / Math.Max(1, PageSize));
    public int DisplayFrom => TotalItems == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
    public int DisplayTo => Math.Min(CurrentPage * PageSize, TotalItems);
    public decimal Subtotal => CartItems.Sum(item => item.LineTotal);
    public decimal Tax => Math.Round(Subtotal * TaxRate, 2);
    public decimal Discount => 0m;
    public decimal Total => Subtotal + Tax - Discount;
    public int CartCount => CartItems.Sum(item => item.Quantity);
    public bool HasCartItems => CartItems.Count > 0;

    private async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
        await LoadProductOptionsAsync();
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = string.Empty;

            var result = await _sportItemService.SearchForPosAsync(
                CurrentPage,
                PageSize,
                Normalize(SearchText),
                ResolveSelectedCategoryId());

            Products = new ObservableCollection<SportItemListRow>(result.Items);
            TotalItems = result.TotalCount;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load POS products: {ex.Message}";
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
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage >= TotalPages)
        {
            return;
        }

        CurrentPage++;
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage <= 1)
        {
            return;
        }

        CurrentPage--;
        await LoadProductsAsync();
    }

    [RelayCommand]
    private void AddProduct(SportItemListRow? product)
    {
        if (product is null)
        {
            return;
        }

        var stock = product.Item.EffectiveStockQuantity;
        if (stock <= 0)
        {
            StatusMessage = $"{product.Item.Name} is out of stock.";
            return;
        }

        var existing = CartItems.FirstOrDefault(item => item.ItemId == product.Item.Id);
        if (existing is not null)
        {
            if (existing.Quantity >= existing.AvailableStock)
            {
                StatusMessage = $"Only {existing.AvailableStock} available for {existing.Name}.";
                return;
            }

            existing.Quantity++;
            RefreshCartTotals();
            return;
        }

        CartItems.Add(new PosCartItem { Product = product });
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private void IncreaseQuantity(PosCartItem? item)
    {
        if (item is null)
        {
            return;
        }

        item.Quantity++;
        RefreshCartTotals();
    }

    [RelayCommand]
    private void DecreaseQuantity(PosCartItem? item)
    {
        if (item is null)
        {
            return;
        }

        if (item.Quantity <= 1)
        {
            CartItems.Remove(item);
            return;
        }

        item.Quantity--;
        RefreshCartTotals();
    }

    [RelayCommand]
    private void RemoveCartItem(PosCartItem? item)
    {
        if (item is not null)
        {
            CartItems.Remove(item);
        }
    }

    [RelayCommand]
    private void NewOrder()
    {
        CartItems.Clear();
        SelectedCustomer = null;
        CustomerSearchText = string.Empty;
        CustomerName = "Walk-in Customer";
        CustomerPhone = "0000000000";
        CustomerAddress = string.Empty;
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (CartItems.Count == 0)
        {
            StatusMessage = "Add at least one product before checkout.";
            return;
        }

        if (!_currentUserService.UserId.HasValue)
        {
            StatusMessage = "Please sign in before checkout.";
            return;
        }

        try
        {
            IsCheckingOut = true;
            var customer = await ResolveCheckoutCustomerAsync();
            var order = new CustomerOrder
            {
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                CustomerPhone = customer.Phone,
                ShippingAddress = customer.Address,
                OrderType = "AtStore",
                Status = "Completed",
                PaymentStatus = "Paid",
                Notes = "Created from POS"
            };

            var details = CartItems.Select(item => new OrderDetail
            {
                ItemId = item.ItemId,
                ItemName = item.Name,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList();

            var created = await _orderService.CreateOrderAsync(
                order,
                details,
                _currentUserService.UserId.Value,
                _currentUserService.UserEmail ?? "POS");

            await _orderService.UpdateStatusAsync(created.Id, "Completed");
            await _orderService.UpdatePaymentStatusAsync(created.Id, "Paid");

            CartItems.Clear();
            StatusMessage = $"Order #{created.Id} checked out successfully.";
            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Checkout failed: {ex.Message}";
        }
        finally
        {
            IsCheckingOut = false;
        }
    }

    public void UpdateSearchText(string? value)
    {
        SearchText = value ?? string.Empty;
        FilterProductOptions(SearchText);
    }

    public async Task SearchCustomersAsync(string? value)
    {
        CustomerSearchText = value ?? string.Empty;

        var keyword = Normalize(CustomerSearchText);
        if (keyword is null)
        {
            CustomerSuggestions = [];
            return;
        }

        try
        {
            var (customers, _) = await _customerService.GetCustomersAsync(1, 8, keyword);
            CustomerSuggestions = new ObservableCollection<Customer>(customers);
        }
        catch (Exception ex)
        {
            CustomerSuggestions = [];
            StatusMessage = $"Failed to search customers: {ex.Message}";
        }
    }

    public void SelectCustomer(Customer? customer)
    {
        if (customer is null)
        {
            return;
        }

        SelectedCustomer = customer;
        ApplyCustomerToForm(customer);
        StatusMessage = string.Empty;
    }

    public void UseCustomerSearchTextAsName(string? value)
    {
        var keyword = Normalize(value);
        if (keyword is null)
        {
            return;
        }

        SelectedCustomer = null;
        CustomerSearchText = keyword;
        CustomerName = keyword;
    }

    partial void OnCustomerNameChanged(string value) => ClearSelectedCustomerIfFormChanged();

    partial void OnCustomerPhoneChanged(string value) => ClearSelectedCustomerIfFormChanged();

    partial void OnCustomerAddressChanged(string value) => ClearSelectedCustomerIfFormChanged();

    public async Task UpdateCategoryAsync(string? value)
    {
        SelectedCategory = string.IsNullOrWhiteSpace(value) ? "All Gear" : value;
        SearchText = string.Empty;
        await LoadProductOptionsAsync();
        await SearchAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = await _categoryService.GetAllAsync();
            _categoryIdsByName.Clear();
            foreach (var category in categories)
            {
                if (!string.IsNullOrWhiteSpace(category.Name))
                {
                    _categoryIdsByName[category.Name] = category.Id;
                }
            }

            CategoryOptions = ["All Gear", .. categories.Select(category => category.Name).Where(name => !string.IsNullOrWhiteSpace(name)).OrderBy(name => name)];
        }
        catch
        {
            CategoryOptions = ["All Gear"];
        }
    }

    private async Task LoadProductOptionsAsync()
    {
        try
        {
            _availableProductOptions = await _sportItemService.GetProductNamesAsync(ResolveSelectedCategoryId());
            FilterProductOptions(SearchText);
        }
        catch
        {
            _availableProductOptions = [];
            ProductOptions = [];
        }
    }

    private int? ResolveSelectedCategoryId()
        => SelectedCategory != "All Gear" && _categoryIdsByName.TryGetValue(SelectedCategory, out var categoryId)
            ? categoryId
            : null;

    private void FilterProductOptions(string? value)
    {
        var keyword = Normalize(value);
        ProductOptions = keyword is null
            ? [.. _availableProductOptions]
            : [.. _availableProductOptions.Where(name => name.Contains(keyword, StringComparison.OrdinalIgnoreCase))];
    }

    private void CartItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (PosCartItem item in e.OldItems)
            {
                item.PropertyChanged -= CartItem_PropertyChanged;
            }
        }

        if (e.NewItems is not null)
        {
            foreach (PosCartItem item in e.NewItems)
            {
                item.PropertyChanged += CartItem_PropertyChanged;
            }
        }

        RefreshCartTotals();
    }

    private void CartItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PosCartItem.Quantity) || e.PropertyName == nameof(PosCartItem.LineTotal))
        {
            RefreshCartTotals();
        }
    }

    private void RefreshCartTotals()
    {
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(Tax));
        OnPropertyChanged(nameof(Discount));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(CartCount));
        OnPropertyChanged(nameof(HasCartItems));
    }

    private async Task<Customer> ResolveCheckoutCustomerAsync()
    {
        var name = Normalize(CustomerName) ?? "Walk-in Customer";
        var phone = Normalize(CustomerPhone) ?? "0000000000";
        var address = Normalize(CustomerAddress);

        if (SelectedCustomer is not null
            && string.Equals(SelectedCustomer.Name, name, StringComparison.Ordinal)
            && string.Equals(SelectedCustomer.Phone, phone, StringComparison.Ordinal)
            && string.Equals(SelectedCustomer.Address ?? string.Empty, address ?? string.Empty, StringComparison.Ordinal))
        {
            return SelectedCustomer;
        }

        var existing = await _customerService.GetCustomerByPhoneAsync(phone);
        if (existing is not null)
        {
            SelectedCustomer = existing;
            ApplyCustomerToForm(existing);
            return existing;
        }

        var createdCustomer = new Customer
        {
            Name = name,
            Phone = phone,
            Address = address
        };

        createdCustomer.Id = await _customerService.SaveCustomerAsync(createdCustomer);
        SelectedCustomer = createdCustomer;
        ApplyCustomerToForm(createdCustomer);
        return createdCustomer;
    }

    private void ApplyCustomerToForm(Customer customer)
    {
        _isApplyingCustomer = true;
        CustomerSearchText = customer.Name;
        CustomerName = customer.Name;
        CustomerPhone = customer.Phone;
        CustomerAddress = customer.Address ?? string.Empty;
        _isApplyingCustomer = false;
    }

    private void ClearSelectedCustomerIfFormChanged()
    {
        if (_isApplyingCustomer || SelectedCustomer is null)
        {
            return;
        }

        var address = Normalize(CustomerAddress) ?? string.Empty;
        if (!string.Equals(SelectedCustomer.Name, CustomerName, StringComparison.Ordinal)
            || !string.Equals(SelectedCustomer.Phone, CustomerPhone, StringComparison.Ordinal)
            || !string.Equals(SelectedCustomer.Address ?? string.Empty, address, StringComparison.Ordinal))
        {
            SelectedCustomer = null;
        }
    }

    private static string? Normalize(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
