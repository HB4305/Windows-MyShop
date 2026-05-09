using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using MyShop.Controls;
using MyShop.Models;
using MyShop.ViewModels;
using MyShop.Views.Dialogs;

namespace MyShop.Views;

public sealed partial class PosPage : Page
{
    private const double StackedBreakpoint = 1120;

    public PosPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<PosViewModel>();
        if (ViewModel is not null)
        {
            ViewModel.ShowCheckoutResultDialogAsync = ShowCheckoutResultDialogAsync;
        }
    }

    private PosViewModel? ViewModel => DataContext as PosViewModel;

    private async Task ShowCheckoutResultDialogAsync(string title, string content, bool isSuccess)
    {
        if (isSuccess)
        {
            var dialog = new SuccessDialog(title, content)
            {
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
            return;
        }

        var errorDialog = new ErrorDialog(title, content)
        {
            XamlRoot = XamlRoot
        };
        await errorDialog.ShowAsync();
    }

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
            var selectedCategory = args.AddedItems.FirstOrDefault()?.ToString() ?? sender.SelectedCategoryText;
            await ViewModel.UpdateCategoryAsync(selectedCategory);
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

    private async void CustomerSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion is Customer customer)
        {
            ViewModel?.SelectCustomer(customer);
            return;
        }

        if (ViewModel is not null)
        {
            await ViewModel.UseCustomerSearchTextAsNameAsync(sender.Text);
        }
    }

    private void Page_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject originalSource)
        {
            ProductFilterBar.CloseFlyouts();
            CustomerSearchBox.IsSuggestionListOpen = false;
            return;
        }

        if (!IsDescendantOf(originalSource, ProductFilterBar))
        {
            ProductFilterBar.CloseFlyouts();
        }

        if (!IsDescendantOf(originalSource, CustomerSearchBox))
        {
            CustomerSearchBox.IsSuggestionListOpen = false;
        }
    }

    private static bool IsDescendantOf(DependencyObject? source, DependencyObject ancestor)
    {
        while (source is not null)
        {
            if (ReferenceEquals(source, ancestor))
            {
                return true;
            }

            source = VisualTreeHelper.GetParent(source);
        }

        return false;
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
