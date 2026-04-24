using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using MyShop.Models;
using MyShop.Services;
using MyShop.ViewModels;
using MyShop.Views.Dialogs;

namespace MyShop.Views;

public sealed partial class CustomerPage : Page
{
    public CustomerViewModel ViewModel { get; }
    private bool _isDialogBusy = false;

    public CustomerPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<CustomerViewModel>();
        DataContext = ViewModel;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadCustomersAsync();
    }

    private void OnSearchKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            ViewModel.SearchCommand.Execute(null);
        }
    }

    private async void OnAddCustomerClick(object sender, RoutedEventArgs e)
    {
        if (_isDialogBusy) return;
        _isDialogBusy = true;
        GlobalLoadingOverlay.Visibility = Visibility.Visible;

        try
        {
            var dialog = new AddEditCustomerDialog() { XamlRoot = this.XamlRoot };
            GlobalLoadingOverlay.Visibility = Visibility.Collapsed; // Hide before showing dialog
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                var customer = dialog.GetCustomer();
                var service = App.Services.GetRequiredService<CustomerService>();
                await service.SaveCustomerAsync(customer);
                await ViewModel.LoadCustomersAsync(); 
            }
        }
        finally
        {
            _isDialogBusy = false;
            GlobalLoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private async void OnEditCustomerClick(object sender, RoutedEventArgs e)
    {
        if (_isDialogBusy) return;
        _isDialogBusy = true;
        GlobalLoadingOverlay.Visibility = Visibility.Visible;

        try
        {
            var customer = (sender as FrameworkElement)?.DataContext as Customer;
            if (customer == null) return;

            var dialog = new AddEditCustomerDialog(customer) { XamlRoot = this.XamlRoot };
            GlobalLoadingOverlay.Visibility = Visibility.Collapsed; // Hide before showing dialog
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                var edited = dialog.GetCustomer();
                var service = App.Services.GetRequiredService<CustomerService>();
                await service.SaveCustomerAsync(edited);
                await ViewModel.LoadCustomersAsync();
            }
        }
        finally
        {
            _isDialogBusy = false;
            GlobalLoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }


    private async void OnDeleteCustomerClick(object sender, RoutedEventArgs e)
    {
        var customer = (sender as FrameworkElement)?.DataContext as Customer;
        if (customer == null) return;

        var dialog = new ConfirmationDialog(
            "Delete Customer",
            $"Are you sure you want to delete {customer.Name}? This will not delete their order history but they will no longer be linked in the directory.")
        {
            XamlRoot = this.XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            await ViewModel.DeleteCustomerCommand.ExecuteAsync(customer);
        }
    }

    private async void OnCustomerClick(object sender, ItemClickEventArgs e)
    {
        if (_isDialogBusy) return;
        _isDialogBusy = true;
        GlobalLoadingOverlay.Visibility = Visibility.Visible;

        try
        {
            var customer = e.ClickedItem as Customer;
            if (customer == null) return;

            var service = App.Services.GetRequiredService<CustomerService>();
            var history = await service.GetCustomerOrderHistoryAsync(customer.Id);

            var dialog = new CustomerDetailDialog(customer, history) { XamlRoot = this.XamlRoot };
            GlobalLoadingOverlay.Visibility = Visibility.Collapsed; // Hide before showing dialog
            await dialog.ShowAsync();
        }
        finally
        {
            _isDialogBusy = false;
            GlobalLoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }

}
