using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MyShop.Models;
using MyShop.ViewModels;
using MyShop.Views.Dialogs;

namespace MyShop.Views;

public sealed partial class SuppliersPage : Page
{
    public SuppliersViewModel ViewModel { get; }

    public SuppliersPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<SuppliersViewModel>();
        this.DataContext = ViewModel;
        Loaded += SuppliersPage_Loaded;
    }

    private async void SuppliersPage_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadSuppliersAsync();
        await ViewModel.LoadSupplyOrdersAsync();
    }

    private void TabSuppliers_Click(object sender, RoutedEventArgs e)
    {
        SuppliersGrid.Visibility = Visibility.Visible;
        SupplyOrdersGrid.Visibility = Visibility.Collapsed;

        // Active style
        TabSuppliers.Background = (SolidColorBrush)Application.Current.Resources["AccentFillColorDefaultBrush"];
        TabSuppliers.Foreground = new SolidColorBrush(Microsoft.UI.Colors.White);
        TabSuppliers.BorderThickness = new Thickness(0);

        // Inactive style
        TabSupplyOrders.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        TabSupplyOrders.Foreground = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
        TabSupplyOrders.BorderThickness = new Thickness(1);
    }

    private void TabSupplyOrders_Click(object sender, RoutedEventArgs e)
    {
        SuppliersGrid.Visibility = Visibility.Collapsed;
        SupplyOrdersGrid.Visibility = Visibility.Visible;

        // Active style
        TabSupplyOrders.Background = (SolidColorBrush)Application.Current.Resources["AccentFillColorDefaultBrush"];
        TabSupplyOrders.Foreground = new SolidColorBrush(Microsoft.UI.Colors.White);
        TabSupplyOrders.BorderThickness = new Thickness(0);

        // Inactive style
        TabSuppliers.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        TabSuppliers.Foreground = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
        TabSuppliers.BorderThickness = new Thickness(1);
    }

    private async void AddSupplier_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SupplierDialog(null)
        {
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await ViewModel.LoadSuppliersAsync();
        }
    }

    private async void EditSupplier_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Supplier supplier)
        {
            var dialog = new SupplierDialog(supplier)
            {
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.LoadSuppliersAsync();
            }
        }
    }

    private async void CreateSupplyOrder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CreateSupplyOrderDialog()
        {
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await ViewModel.LoadSupplyOrdersAsync();
            await ViewModel.LoadSuppliersAsync(); // if needed
        }
    }
}
