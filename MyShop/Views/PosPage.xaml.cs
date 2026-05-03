using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MyShop.Controls;
using MyShop.Models;
using MyShop.ViewModels;

namespace MyShop.Views;

public sealed partial class PosPage : Page
{
    private const double StackedBreakpoint = 1120;

    public PosPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<PosViewModel>();
    }

    private PosViewModel? ViewModel => DataContext as PosViewModel;

    private void ProductsGrid_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is SportItemListRow product)
        {
            ViewModel?.AddProductCommand.Execute(product);
        }
    }

    private void IncreaseCartItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: PosCartItem item })
        {
            ViewModel?.IncreaseQuantityCommand.Execute(item);
        }
    }

    private void DecreaseCartItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: PosCartItem item })
        {
            ViewModel?.DecreaseQuantityCommand.Execute(item);
        }
    }

    private void RemoveCartItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: PosCartItem item })
        {
            ViewModel?.RemoveCartItemCommand.Execute(item);
        }
    }

    private void ProductFilterBar_SearchTextChanged(ProductSearchFilterBar sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            ViewModel?.UpdateSearchText(sender.SearchText);
        }
    }

    private void ProductFilterBar_SearchSuggestionChosen(ProductSearchFilterBar sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if (args.SelectedItem is string selectedProduct)
        {
            ViewModel?.UpdateSearchText(selectedProduct);
        }
    }

    private void ProductFilterBar_SearchQuerySubmitted(ProductSearchFilterBar sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        ViewModel?.UpdateSearchText(args.ChosenSuggestion?.ToString() ?? sender.SearchText);
    }

    private async void ProductFilterBar_CategorySelectionChanged(ProductSearchFilterBar sender, SelectionChangedEventArgs args)
    {
        if (ViewModel is not null)
        {
            await ViewModel.UpdateCategoryAsync(sender.SelectedCategory?.ToString());
        }
    }

    private async void CustomerSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && ViewModel is not null)
        {
            await ViewModel.SearchCustomersAsync(sender.Text);
        }
    }

    private void CustomerSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if (args.SelectedItem is Customer customer)
        {
            ViewModel?.SelectCustomer(customer);
        }
    }

    private void CustomerSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion is Customer customer)
        {
            ViewModel?.SelectCustomer(customer);
            return;
        }

        ViewModel?.UseCustomerSearchTextAsName(sender.Text);
    }

    private void Page_Tapped(object sender, TappedRoutedEventArgs e)
    {
        ProductFilterBar.CloseFlyouts();
        CustomerSearchBox.IsSuggestionListOpen = false;
    }

    private void RootLayout_Loaded(object sender, RoutedEventArgs e)
    {
        ApplyResponsiveLayout(RootLayout.ActualWidth);
    }

    private void RootLayout_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyResponsiveLayout(e.NewSize.Width);
    }

    private void ApplyResponsiveLayout(double width)
    {
        if (width < StackedBreakpoint)
        {
            SetStackedLayout();
            return;
        }

        SetWideLayout();
    }

    private void SetWideLayout()
    {
        RootLayout.ColumnDefinitions.Clear();
        RootLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
        RootLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(400) });

        RootLayout.RowDefinitions.Clear();
        RootLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        RootLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        RootLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        Grid.SetRow(HeaderPanel, 0);
        Grid.SetColumn(HeaderPanel, 0);
        Grid.SetColumnSpan(HeaderPanel, 2);

        Grid.SetRow(ProductPanel, 1);
        Grid.SetColumn(ProductPanel, 0);
        Grid.SetColumnSpan(ProductPanel, 1);

        Grid.SetRow(CheckoutPanel, 1);
        Grid.SetColumn(CheckoutPanel, 1);
        Grid.SetColumnSpan(CheckoutPanel, 1);

        Grid.SetRow(StatusText, 2);
        Grid.SetColumn(StatusText, 0);
        Grid.SetColumnSpan(StatusText, 2);
    }

    private void SetStackedLayout()
    {
        RootLayout.ColumnDefinitions.Clear();
        RootLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        RootLayout.RowDefinitions.Clear();
        RootLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        RootLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        RootLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        RootLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        Grid.SetRow(HeaderPanel, 0);
        Grid.SetColumn(HeaderPanel, 0);
        Grid.SetColumnSpan(HeaderPanel, 1);

        Grid.SetRow(ProductPanel, 1);
        Grid.SetColumn(ProductPanel, 0);
        Grid.SetColumnSpan(ProductPanel, 1);

        Grid.SetRow(CheckoutPanel, 2);
        Grid.SetColumn(CheckoutPanel, 0);
        Grid.SetColumnSpan(CheckoutPanel, 1);

        Grid.SetRow(StatusText, 3);
        Grid.SetColumn(StatusText, 0);
        Grid.SetColumnSpan(StatusText, 1);
    }
}
