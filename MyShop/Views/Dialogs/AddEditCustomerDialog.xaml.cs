using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyShop.Views.Dialogs;

public sealed partial class AddEditCustomerDialog : ContentDialog, INotifyPropertyChanged
{
    private Customer? _customer;
    private string _customerName = string.Empty;
    private string _customerPhone = string.Empty;
    private string? _customerAddress;
    private string _nameValidationMessage = string.Empty;
    private string _phoneValidationMessage = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string CustomerName
    {
        get => _customerName;
        set { SetProperty(ref _customerName, value); ValidateName(); }
    }

    public string CustomerPhone
    {
        get => _customerPhone;
        set { SetProperty(ref _customerPhone, value); ValidatePhone(); }
    }

    public string? CustomerAddress
    {
        get => _customerAddress;
        set => SetProperty(ref _customerAddress, value);
    }

    public string NameValidationMessage
    {
        get => _nameValidationMessage;
        private set => SetProperty(ref _nameValidationMessage, value);
    }

    public string PhoneValidationMessage
    {
        get => _phoneValidationMessage;
        private set => SetProperty(ref _phoneValidationMessage, value);
    }

    public AddEditCustomerDialog(Customer? customer = null)
    {
        this.InitializeComponent();
        _customer = customer;

        if (_customer != null)
        {
            FormTitle.Text = "Edit Customer";
            SaveBtnText.Text = "Update Customer";
            CustomerName = _customer.Name;
            CustomerPhone = _customer.Phone;
            CustomerAddress = _customer.Address;
        }
        else
        {
            FormTitle.Text = "New Customer";
            SaveBtnText.Text = "Create Customer";
        }
    }

    private void ValidateName()
    {
        NameValidationMessage = string.IsNullOrWhiteSpace(CustomerName) ? "Name is required" : string.Empty;
    }

    private void ValidatePhone()
    {
        if (string.IsNullOrWhiteSpace(CustomerPhone))
            PhoneValidationMessage = "Phone number is required";
        else if (CustomerPhone.Length < 10)
            PhoneValidationMessage = "Phone number must be at least 10 digits";
        else
            PhoneValidationMessage = string.Empty;
    }

    private void SaveBtn_Click(object sender, RoutedEventArgs e)
    {
        ValidateName();
        ValidatePhone();

        if (!string.IsNullOrEmpty(NameValidationMessage) || !string.IsNullOrEmpty(PhoneValidationMessage))
            return;

        if (_customer == null) _customer = new Customer();
        _customer.Name = CustomerName;
        _customer.Phone = CustomerPhone;
        _customer.Address = CustomerAddress;

        // Note: Result is handled by returning Primary in the ShowAsync pattern 
        // but since we are using Hide(), we need to signal success.
        this.Result = ContentDialogResult.Primary;
        Hide();
    }

    private void CancelBtn_Click(object sender, RoutedEventArgs e)
    {
        this.Result = ContentDialogResult.None;
        Hide();
    }

    public Customer GetCustomer() => _customer ?? new Customer();

    public ContentDialogResult Result { get; private set; } = ContentDialogResult.None;

    public new async Task<ContentDialogResult> ShowAsync()
    {
        await base.ShowAsync();
        return Result;
    }

    private void SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(storage, value)) return;
        storage = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
