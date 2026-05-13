using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MyShop.Models;
using MyShop.ViewModels;
using MyShop.Views.Dialogs;

namespace MyShop.Views;

public sealed partial class CustomerPage : Page
{
    public CustomerViewModel ViewModel { get; }

    public CustomerPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<CustomerViewModel>();
        this.DataContext = ViewModel;

        Loaded += CustomerPage_Loaded;
        
        ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.CurrentPage) || e.PropertyName == nameof(ViewModel.TotalItems))
            {
                BuildPagination();
            }
        };
    }

    private async void CustomerPage_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadCustomersAsync();
        BuildPagination();
    }

    private void OnSearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        ViewModel.SearchCommand.Execute(null);
    }

    private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
    {
        SearchWrapper.BorderBrush = (Brush)Application.Current.Resources["AppPurpleBrush"];
    }

    private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
    {
        SearchWrapper.BorderBrush = (Brush)Application.Current.Resources["ControlStrokeColorDefaultBrush"];
    }

    private async void OnAddCustomerClick(object sender, RoutedEventArgs e)
    {
        GlobalLoadingOverlay.Visibility = Visibility.Visible;
        try
        {
            var dialog = new AddEditCustomerDialog(null) { XamlRoot = this.XamlRoot };
            GlobalLoadingOverlay.Visibility = Visibility.Collapsed;
            if (await dialog.ShowAsync() == ContentDialogResult.Primary) await ViewModel.LoadCustomersAsync();
        }
        finally { GlobalLoadingOverlay.Visibility = Visibility.Collapsed; }
    }

    private async void OnEditCustomerClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Customer customer)
        {
            var dialog = new AddEditCustomerDialog(customer) { XamlRoot = this.XamlRoot };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary) await ViewModel.LoadCustomersAsync();
        }
    }

    private async void OnDeleteCustomerClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Customer customer)
        {
            var dialog = new ConfirmationDialog("Delete Customer", $"Are you sure you want to delete {customer.Name}?");
            dialog.XamlRoot = this.XamlRoot;
            if (await dialog.ShowAsync() == ContentDialogResult.Primary) await ViewModel.DeleteCustomerAsync(customer);
        }
    }

    private void OnCustomerClick(object sender, ItemClickEventArgs e)
    {
        // Navigate to details if implemented
    }

    private void BuildPagination()
    {
        if (CustomerPaginationPanel == null) return;
        CustomerPaginationPanel.Children.Clear();

        var total = ViewModel.TotalPages;
        var current = ViewModel.CurrentPage;

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
                ViewModel.CurrentPage = page;
                _ = ViewModel.LoadCustomersAsync(); 
                BuildPagination();
            };

            CustomerPaginationPanel.Children.Add(btn);
        }

        // Prev button
        var prevBtn = new Button
        {
            Content = new TextBlock { Text = "Prev", FontSize = 13 },
            MinWidth = 52,
            Height = 32,
            Padding = new Thickness(8, 4, 8, 4),
            Style = (Style)Resources["PageBtn"],
            IsEnabled = current > 1,
        };
        prevBtn.Click += (s, e) => { if (current > 1) { ViewModel.CurrentPage--; _ = ViewModel.LoadCustomersAsync(); BuildPagination(); } };
        CustomerPaginationPanel.Children.Add(prevBtn);

        // Page numbers logic (smart ellipsis)
        if (total <= 7)
        {
            for (int i = 1; i <= total; i++)
                AddPageBtn(i, i == current);
        }
        else
        {
            AddPageBtn(1, current == 1);
            if (current > 3) CustomerPaginationPanel.Children.Add(new TextBlock { Text = "...", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 4, 0), Foreground = (Microsoft.UI.Xaml.Media.Brush)Resources["TextFillColorSecondaryBrush"] });

            int start = Math.Max(2, current - 1);
            int end = Math.Min(total - 1, current + 1);

            for (int i = start; i <= end; i++)
                AddPageBtn(i, i == current);

            if (current < total - 2) CustomerPaginationPanel.Children.Add(new TextBlock { Text = "...", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 4, 0), Foreground = (Microsoft.UI.Xaml.Media.Brush)Resources["TextFillColorSecondaryBrush"] });
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
            IsEnabled = current < total,
        };
        nextBtn.Click += (s, e) => { if (current < total) { ViewModel.CurrentPage++; _ = ViewModel.LoadCustomersAsync(); BuildPagination(); } };
        CustomerPaginationPanel.Children.Add(nextBtn);
    }
}
