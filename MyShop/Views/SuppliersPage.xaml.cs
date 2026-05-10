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

        ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.SupplierCurrentPage) || e.PropertyName == nameof(ViewModel.SupplierTotalItems))
                BuildSupplierPagination();
            
            if (e.PropertyName == nameof(ViewModel.OrderCurrentPage) || e.PropertyName == nameof(ViewModel.OrderTotalItems))
                BuildOrderPagination();
        };
    }

    private async void SuppliersPage_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadSuppliersAsync();
        await ViewModel.LoadSupplyOrdersAsync();
        BuildSupplierPagination();
        BuildOrderPagination();
    }

    private void TabSuppliers_Click(object sender, RoutedEventArgs e)
    {
        SuppliersGrid.Visibility = Visibility.Visible;
        SupplyOrdersGrid.Visibility = Visibility.Collapsed;

        TabSuppliers.Background = (SolidColorBrush)Application.Current.Resources["PurpleBrush"];
        TabSuppliers.Foreground = new SolidColorBrush(Microsoft.UI.Colors.White);
        TabSuppliers.BorderThickness = new Thickness(0);

        TabSupplyOrders.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        TabSupplyOrders.Foreground = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
        TabSupplyOrders.BorderThickness = new Thickness(1);
        
        BuildSupplierPagination();
    }

    private void TabSupplyOrders_Click(object sender, RoutedEventArgs e)
    {
        SuppliersGrid.Visibility = Visibility.Collapsed;
        SupplyOrdersGrid.Visibility = Visibility.Visible;

        TabSupplyOrders.Background = (SolidColorBrush)Application.Current.Resources["PurpleBrush"];
        TabSupplyOrders.Foreground = new SolidColorBrush(Microsoft.UI.Colors.White);
        TabSupplyOrders.BorderThickness = new Thickness(0);

        TabSuppliers.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        TabSuppliers.Foreground = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
        TabSuppliers.BorderThickness = new Thickness(1);
        
        BuildOrderPagination();
    }

    private async void AddSupplier_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SupplierDialog(null) { XamlRoot = this.XamlRoot };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary) await ViewModel.LoadSuppliersAsync();
    }

    private async void EditSupplier_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Supplier supplier)
        {
            var dialog = new SupplierDialog(supplier) { XamlRoot = this.XamlRoot };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary) await ViewModel.LoadSuppliersAsync();
        }
    }

    private async void CreateSupplyOrder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CreateSupplyOrderDialog() { XamlRoot = this.XamlRoot };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            await ViewModel.LoadSupplyOrdersAsync();
            await ViewModel.LoadSuppliersAsync();
        }
    }

    private void BuildSupplierPagination()
    {
        if (SupplierPaginationPanel == null) return;
        SupplierPaginationPanel.Children.Clear();

        var total = ViewModel.SupplierTotalPages;
        var current = ViewModel.SupplierCurrentPage;

        void AddPageBtn(int page, bool isActive)
        {
            var btn = new Button
            {
                Content = page.ToString(),
                MinWidth = 32,
                Height = 32,
                Padding = new Thickness(8, 4, 8, 4),
                FontSize = 13,
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(2, 0, 2, 0),
            };

            if (isActive)
            {
                btn.Style = (Style)Resources["PageBtnActive"];
            }
            else
            {
                btn.Style = (Style)Resources["PageBtn"];
            }

            btn.Click += (s, e) =>
            {
                ViewModel.SupplierCurrentPage = page;
                _ = ViewModel.LoadSuppliersAsync(); 
                BuildSupplierPagination();
            };

            SupplierPaginationPanel.Children.Add(btn);
        }

        // Prev button
        var prevBtn = new Button
        {
            Content = new TextBlock { Text = "Prev", FontSize = 13 },
            MinWidth = 52,
            Height = 32,
            Padding = new Thickness(8, 4, 8, 4),
            Style = (Style)Resources["PageBtn"],
        };
        prevBtn.Click += (s, e) => { if (current > 1) { ViewModel.SupplierCurrentPage--; _ = ViewModel.LoadSuppliersAsync(); BuildSupplierPagination(); } };
        SupplierPaginationPanel.Children.Add(prevBtn);

        // Page numbers logic (smart ellipsis)
        if (total <= 7)
        {
            for (int i = 1; i <= total; i++)
                AddPageBtn(i, i == current);
        }
        else
        {
            AddPageBtn(1, current == 1);
            if (current > 3) SupplierPaginationPanel.Children.Add(new TextBlock { Text = "...", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 4, 0), Foreground = (Brush)Resources["TextFillColorSecondaryBrush"] });

            int start = Math.Max(2, current - 1);
            int end = Math.Min(total - 1, current + 1);

            for (int i = start; i <= end; i++)
                AddPageBtn(i, i == current);

            if (current < total - 2) SupplierPaginationPanel.Children.Add(new TextBlock { Text = "...", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 4, 0), Foreground = (Brush)Resources["TextFillColorSecondaryBrush"] });
            AddPageBtn(total, current == total);
        }

        // Next button
        var nextBtn = new Button
        {
            Content = new TextBlock { Text = "Next", FontSize = 13 },
            MinWidth = 52,
            Height = 32,
            Padding = new Thickness(8, 4, 8, 4),
            Style = (Style)Resources["PageBtn"],
        };
        nextBtn.Click += (s, e) => { if (current < total) { ViewModel.SupplierCurrentPage++; _ = ViewModel.LoadSuppliersAsync(); BuildSupplierPagination(); } };
        SupplierPaginationPanel.Children.Add(nextBtn);
    }

    private void BuildOrderPagination()
    {
        if (OrderPaginationPanel == null) return;
        OrderPaginationPanel.Children.Clear();

        var total = ViewModel.OrderTotalPages;
        var current = ViewModel.OrderCurrentPage;

        void AddPageBtn(int page, bool isActive)
        {
            var btn = new Button
            {
                Content = page.ToString(),
                MinWidth = 32,
                Height = 32,
                Padding = new Thickness(8, 4, 8, 4),
                FontSize = 13,
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(2, 0, 2, 0),
            };

            if (isActive)
            {
                btn.Style = (Style)Resources["PageBtnActive"];
            }
            else
            {
                btn.Style = (Style)Resources["PageBtn"];
            }

            btn.Click += (s, e) =>
            {
                ViewModel.OrderCurrentPage = page;
                _ = ViewModel.LoadSupplyOrdersAsync(); 
                BuildOrderPagination();
            };

            OrderPaginationPanel.Children.Add(btn);
        }

        // Prev button
        var prevBtn = new Button
        {
            Content = new TextBlock { Text = "Prev", FontSize = 13 },
            MinWidth = 52,
            Height = 32,
            Padding = new Thickness(8, 4, 8, 4),
            Style = (Style)Resources["PageBtn"],
        };
        prevBtn.Click += (s, e) => { if (current > 1) { ViewModel.OrderCurrentPage--; _ = ViewModel.LoadSupplyOrdersAsync(); BuildOrderPagination(); } };
        OrderPaginationPanel.Children.Add(prevBtn);

        // Page numbers logic (smart ellipsis)
        if (total <= 7)
        {
            for (int i = 1; i <= total; i++)
                AddPageBtn(i, i == current);
        }
        else
        {
            AddPageBtn(1, current == 1);
            if (current > 3) OrderPaginationPanel.Children.Add(new TextBlock { Text = "...", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 4, 0), Foreground = (Brush)Resources["TextFillColorSecondaryBrush"] });

            int start = Math.Max(2, current - 1);
            int end = Math.Min(total - 1, current + 1);

            for (int i = start; i <= end; i++)
                AddPageBtn(i, i == current);

            if (current < total - 2) OrderPaginationPanel.Children.Add(new TextBlock { Text = "...", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 4, 0), Foreground = (Brush)Resources["TextFillColorSecondaryBrush"] });
            AddPageBtn(total, current == total);
        }

        // Next button
        var nextBtn = new Button
        {
            Content = new TextBlock { Text = "Next", FontSize = 13 },
            MinWidth = 52,
            Height = 32,
            Padding = new Thickness(8, 4, 8, 4),
            Style = (Style)Resources["PageBtn"],
        };
        nextBtn.Click += (s, e) => { if (current < total) { ViewModel.OrderCurrentPage++; _ = ViewModel.LoadSupplyOrdersAsync(); BuildOrderPagination(); } };
        OrderPaginationPanel.Children.Add(nextBtn);
    }
}

