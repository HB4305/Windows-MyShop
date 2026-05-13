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
    private const double StackedBreakpoint = 1100;

    public PosPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<PosViewModel>();
        if (ViewModel is not null)
        {
            ViewModel.ShowCheckoutResultDialogAsync = ShowCheckoutResultDialogAsync;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
        Loaded += (s, e) => BuildPagination();
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PosViewModel.CurrentPage) ||
            e.PropertyName == nameof(PosViewModel.TotalPages))
        {
            BuildPagination();
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

    private void CustomerSearchBox_GotFocus(object sender, RoutedEventArgs e)
    {
        CustomerSearchWrapper.BorderBrush = (Brush)Application.Current.Resources["AppPurpleBrush"];
    }

    private void CustomerSearchBox_LostFocus(object sender, RoutedEventArgs e)
    {
        CustomerSearchWrapper.BorderBrush = (Brush)Application.Current.Resources["ControlStrokeColorDefaultBrush"];
    }

    private void PhoneTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        PhoneWrapper.BorderBrush = (Brush)Application.Current.Resources["AppPurpleBrush"];
    }

    private void PhoneTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        PhoneWrapper.BorderBrush = (Brush)Application.Current.Resources["ControlStrokeColorDefaultBrush"];
    }

    private void AddressTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        AddressWrapper.BorderBrush = (Brush)Application.Current.Resources["AppPurpleBrush"];
    }

    private void AddressTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        AddressWrapper.BorderBrush = (Brush)Application.Current.Resources["ControlStrokeColorDefaultBrush"];
    }

    private void PaymentMethodComboBox_GotFocus(object sender, RoutedEventArgs e)
    {
        PaymentMethodWrapper.BorderBrush = (Brush)Application.Current.Resources["AppPurpleBrush"];
    }

    private void PaymentMethodComboBox_LostFocus(object sender, RoutedEventArgs e)
    {
        PaymentMethodWrapper.BorderBrush = (Brush)Application.Current.Resources["ControlStrokeColorDefaultBrush"];
    }

    private void CashReceivedTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        CashReceivedWrapper.BorderBrush = (Brush)Application.Current.Resources["AppPurpleBrush"];
    }

    private void CashReceivedTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        CashReceivedWrapper.BorderBrush = (Brush)Application.Current.Resources["ControlStrokeColorDefaultBrush"];
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
        RootLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        Grid.SetRow(HeaderPanel, 0);
        Grid.SetColumn(HeaderPanel, 0);
        Grid.SetColumnSpan(HeaderPanel, 2);

        Grid.SetRow(ProductPanel, 1);
        Grid.SetColumn(ProductPanel, 0);
        Grid.SetColumnSpan(ProductPanel, 1);

        Grid.SetRow(CheckoutScroller, 1);
        Grid.SetColumn(CheckoutScroller, 1);
        Grid.SetColumnSpan(CheckoutScroller, 1);

        // Pagination only on product list side
        var paginationBorder = (FrameworkElement)VisualTreeHelper.GetParent(PaginationPanel);
        while (paginationBorder != null && paginationBorder is not Border)
            paginationBorder = (FrameworkElement)VisualTreeHelper.GetParent(paginationBorder);

        if (paginationBorder != null)
        {
            Grid.SetRow(paginationBorder, 2);
            Grid.SetColumn(paginationBorder, 0);
            Grid.SetColumnSpan(paginationBorder, 1);
            paginationBorder.Margin = new Thickness(-24, 0, 0, -20);
            paginationBorder.Background = null;
        }

        Grid.SetRow(StatusText, 3);
        Grid.SetColumn(StatusText, 0);
        Grid.SetColumnSpan(StatusText, 1);
        StatusText.Margin = new Thickness(0, 0, 0, -20);
    }

    private void SetStackedLayout()
    {
        RootLayout.ColumnDefinitions.Clear();
        RootLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        RootLayout.RowDefinitions.Clear();
        RootLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        RootLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        RootLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        RootLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        RootLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        Grid.SetRow(HeaderPanel, 0);
        Grid.SetColumn(HeaderPanel, 0);
        Grid.SetColumnSpan(HeaderPanel, 1);

        Grid.SetRow(ProductPanel, 1);
        Grid.SetColumn(ProductPanel, 0);
        Grid.SetColumnSpan(ProductPanel, 1);

        // Pagination 
        var paginationBorder = (FrameworkElement)VisualTreeHelper.GetParent(PaginationPanel);
        while (paginationBorder != null && paginationBorder is not Border)
            paginationBorder = (FrameworkElement)VisualTreeHelper.GetParent(paginationBorder);

        if (paginationBorder != null)
        {
            Grid.SetRow(paginationBorder, 2);
            Grid.SetColumn(paginationBorder, 0);
            Grid.SetColumnSpan(paginationBorder, 1);
            paginationBorder.Margin = new Thickness(-24, 0, -24, 0);
            paginationBorder.Background = null;
        }

        Grid.SetRow(CheckoutScroller, 3);
        Grid.SetColumn(CheckoutScroller, 0);
        Grid.SetColumnSpan(CheckoutScroller, 1);

        Grid.SetRow(StatusText, 4);
        Grid.SetColumn(StatusText, 0);
        Grid.SetColumnSpan(StatusText, 1);
    }

    private void BuildPagination()
    {
        if (ViewModel == null || PaginationPanel == null) return;
        PaginationPanel.Children.Clear();

        var total = ViewModel.TotalPages;
        var current = ViewModel.CurrentPage;

        void AddPageBtn(int page, bool isActive)
        {
            var btn = new Button
            {
                Content = page.ToString(),
                Margin = new Thickness(2, 0, 2, 0)
            };

            var styleKey = isActive ? "PageBtnActive" : "PageBtn";
            if (Application.Current.Resources.TryGetValue(styleKey, out var styleObj) && styleObj is Style s)
            {
                btn.Style = s;
            }
            else if (isActive)
            {
                // Absolute fallback for active color
                btn.Background = (Brush)Application.Current.Resources["AppPurpleBrush"];
                btn.Foreground = new SolidColorBrush(Microsoft.UI.Colors.White);
            }

            if (isActive)
            {
                btn.FontWeight = Microsoft.UI.Text.FontWeights.ExtraBold;
            }

            btn.Click += async (s, e) =>
            {
                ViewModel.CurrentPage = page;
                await ViewModel.LoadProductsCommand.ExecuteAsync(null);
            };

            PaginationPanel.Children.Add(btn);
        }

        // Prev button
        var prevBtn = new Button
        {
            Content = new TextBlock { Text = "Prev", FontSize = 13, FontFamily = new FontFamily("ms-appx:///Assets/Fonts/MomoTrustSans-VariableFont_wght.ttf#Momo Trust Sans") },
            Margin = new Thickness(2, 0, 2, 0),
            IsEnabled = current > 1
        };
        if (Application.Current.Resources.TryGetValue("PageBtn", out var pStyleObj) && pStyleObj is Style ps)
            prevBtn.Style = ps;

        prevBtn.Click += async (s, e) =>
        {
            if (current > 1)
            {
                ViewModel.CurrentPage = current - 1;
                await ViewModel.LoadProductsCommand.ExecuteAsync(null);
            }
        };
        PaginationPanel.Children.Add(prevBtn);

        // Page numbers
        if (total <= 7)
        {
            for (int i = 1; i <= total; i++)
                AddPageBtn(i, i == current);
        }
        else
        {
            AddPageBtn(1, current == 1);
            if (current > 3) PaginationPanel.Children.Add(new TextBlock
            {
                Text = "...",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 4, 0),
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });

            for (int i = Math.Max(2, current - 1); i <= Math.Min(total - 1, current + 1); i++)
                AddPageBtn(i, i == current);

            if (current < total - 2) PaginationPanel.Children.Add(new TextBlock
            {
                Text = "...",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 4, 0),
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });

            AddPageBtn(total, current == total);
        }

        // Next button
        var nextBtn = new Button
        {
            Content = new TextBlock { Text = "Next", FontSize = 13, FontFamily = new FontFamily("ms-appx:///Assets/Fonts/MomoTrustSans-VariableFont_wght.ttf#Momo Trust Sans") },
            Margin = new Thickness(2, 0, 2, 0),
            IsEnabled = current < total
        };
        if (Application.Current.Resources.TryGetValue("PageBtn", out var nStyleObj) && nStyleObj is Style ns)
            nextBtn.Style = ns;

        nextBtn.Click += async (s, e) =>
        {
            if (current < total)
            {
                ViewModel.CurrentPage = current + 1;
                await ViewModel.LoadProductsCommand.ExecuteAsync(null);
            }
        };
        PaginationPanel.Children.Add(nextBtn);
    }
}
