using MyShop.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
namespace MyShop.Views;

public sealed partial class CustomerOrderPage : Page
{
    public CustomerOrderViewModel ViewModel { get; }

    public CustomerOrderPage()
    {
        this.InitializeComponent();
        try
        {
            ViewModel = App.Services.GetRequiredService<CustomerOrderViewModel>();
            DataContext = ViewModel;
            Loaded += async (s, e) => await ViewModel.LoadOrdersCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CustomerOrderPage] Failed to initialize: {ex.Message}");
            throw;
        }
    }
}