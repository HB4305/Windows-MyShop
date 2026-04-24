using Microsoft.UI.Xaml.Controls;
using MyShop.Models;
using MyShop.Services;
using System.Collections.ObjectModel;

namespace MyShop.Views.Dialogs;

public sealed partial class CustomerDetailDialog : ContentDialog
{
    public Customer Customer { get; }
    public ObservableCollection<CustomerOrder> Orders { get; } = [];

    public CustomerDetailDialog(Customer customer, List<CustomerOrder> history)
    {
        this.InitializeComponent();
        Customer = customer;
        foreach (var order in history)
        {
            Orders.Add(order);
        }
    }

    private void CloseBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Hide();
    }
}
