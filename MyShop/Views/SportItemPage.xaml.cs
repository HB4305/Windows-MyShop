using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MyShop.Models;
using MyShop.Services;
using MyShop.Repositories;
using MyShop.ViewModels;
using MyShop.Views.Dialogs;
using Windows.System;
using Microsoft.UI.Xaml.Media;

namespace MyShop.Views;

public sealed partial class SportItemPage : Page
{
    public SportItemPage()
    {
        this.InitializeComponent();
        DataContext = App.Services.GetRequiredService<SportItemViewModel>();
        _ = ViewModel.LoadItemsAsync();

        Loaded += SportItemPage_OnLoaded;

        ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.CurrentPage) || e.PropertyName == nameof(ViewModel.TotalItems))
            {
                BuildPagination();
            }
        };
    }

    private SportItemViewModel ViewModel => (SportItemViewModel)DataContext;

    private void SportItemPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        // Sale roles cannot see the cost price column (import price)
        var currentUser = App.Services.GetRequiredService<CurrentUserService>();
        if (currentUser.IsSale)
        {
            HideImportPriceColumn();
            AddButton.Visibility = Visibility.Collapsed;
            ImportButton.Visibility = Visibility.Collapsed;
        }
        BuildPagination();
    }

    private void BuildPagination()
    {
        if (ProductPaginationPanel == null) return;
        ProductPaginationPanel.Children.Clear();

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
                _ = ViewModel.LoadItemsAsync(); 
                BuildPagination();
            };

            ProductPaginationPanel.Children.Add(btn);
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
        prevBtn.Click += (s, e) => { if (current > 1) { ViewModel.CurrentPage--; _ = ViewModel.LoadItemsAsync(); BuildPagination(); } };
        ProductPaginationPanel.Children.Add(prevBtn);

        // Page numbers logic (smart ellipsis)
        if (total <= 7)
        {
            for (int i = 1; i <= total; i++)
                AddPageBtn(i, i == current);
        }
        else
        {
            AddPageBtn(1, current == 1);
            if (current > 3) ProductPaginationPanel.Children.Add(new TextBlock { Text = "...", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 4, 0), Foreground = (Brush)Resources["TextFillColorSecondaryBrush"] });

            int start = Math.Max(2, current - 1);
            int end = Math.Min(total - 1, current + 1);

            for (int i = start; i <= end; i++)
                AddPageBtn(i, i == current);

            if (current < total - 2) ProductPaginationPanel.Children.Add(new TextBlock { Text = "...", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 4, 0), Foreground = (Brush)Resources["TextFillColorSecondaryBrush"] });
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
        nextBtn.Click += (s, e) => { if (current < total) { ViewModel.CurrentPage++; _ = ViewModel.LoadItemsAsync(); BuildPagination(); } };
        ProductPaginationPanel.Children.Add(nextBtn);
    }

    /// <summary>
    /// Hides the cost price column in the product table.
    /// This column is only visible to the owner.
    /// </summary>
    private void HideImportPriceColumn()
    {
        // Find the GridView/ListView body to change column widths
        var listView = FindChild<ListView>(this);
        if (listView == null) return;

        // Update column definitions for the header row (internal Grid)
        // Header Grid in XAML defines 6 columns.
        // For sales: hide the Price column (index 2) by setting Width=0.
        // However, the best practice is to use a Converter on the binding.
        // Currently: no action taken here as the cost price is NOT in the SportItemPage table
        // (the table only displays selling price).
        //
        // If selling price needs to be hidden for sales, follow the same logic.
        // XAML columns: 0=Product, 1=Category, 2=Price(SELL), 3=Stock, 4=Status, 5=Actions
    }

    /// <summary>
    /// Finds a child element by type.
    /// </summary>
    private static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
    {
        int childCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T result)
                return result;
            var descendant = FindChild<T>(child);
            if (descendant != null)
                return descendant;
        }
        return default;
    }

    private void OnSortComboSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0 || sender is not ComboBox cb)
            return;
        if (cb.SelectedItem is not ComboBoxItem { Tag: string tag } || string.IsNullOrEmpty(tag))
            return;

        ViewModel.SortField = tag;
        _ = ViewModel.ReloadWithCurrentSortAsync();
    }

    private void OnSearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        ViewModel.SearchKeyword = args.QueryText ?? string.Empty;
        ViewModel.SearchCommand.Execute(null);
    }

    private void OnAddItemClick(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(SportItemDetailPage), null);
    }

    private async void OnBulkImportClick(object sender, RoutedEventArgs e)
    {
        var filePicker = App.Services.GetRequiredService<IFilePickerService>();
        var path = await filePicker.PickOpenFileAsync("Excel Files", new[] { ".xlsx" });
        
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            var excelService = new ExcelImportService();
            await using var stream = File.OpenRead(path);
            var parsedRows = excelService.ParseExcel(stream);

            var validRows = parsedRows.Where(r => r.IsValid).ToList();
            if (validRows.Count == 0)
            {
                var errorDialog = new ConfirmationDialog("Bulk Import", "The selected file contains no valid product rows.");
                errorDialog.XamlRoot = this.XamlRoot;
                await errorDialog.ShowAsync();
                return;
            }

            var confirmDialog = new BulkImportDialog()
            {
                XamlRoot = this.XamlRoot,
                Message = $"Do you want to import {validRows.Count} product(s) from \"{Path.GetFileName(path)}\"?"
            };
            await confirmDialog.ShowAsync();
            var result = confirmDialog.Result;

            if (result == ContentDialogResult.Primary)
            {
                var itemRepo = App.Services.GetRequiredService<SportItemRepository>();
                var categoryRepo = App.Services.GetRequiredService<CategoryRepository>();

                var categories = await categoryRepo.GetAllAsync();
                var categoryMap = categories
                    .Where(c => c.Name != null)
                    .ToDictionary(c => c.Name.Trim(), c => c.Id, System.StringComparer.OrdinalIgnoreCase);

                var items = new List<SportItem>();
                foreach (var row in validRows)
                {
                    int catId = 0;
                    if (!string.IsNullOrWhiteSpace(row.CategoryName))
                    {
                        var trimmedCat = row.CategoryName.Trim();
                        if (categoryMap.TryGetValue(trimmedCat, out catId))
                        {
                            // Found!
                        }
                        else
                        {
                            // Add to DB!
                            var newCat = new Category { Name = trimmedCat };
                            catId = await categoryRepo.AddAsync(newCat);
                            categoryMap[trimmedCat] = catId;
                        }
                    }
                    items.Add(row.ToSportItem(catId));
                }

                var (inserted, errors) = await itemRepo.BulkInsertAsync(items, categoryMap);

                var successDialog = new SuccessDialog(
                    "Bulk Import Success",
                    $"{inserted} product(s) imported successfully!"
                );
                successDialog.XamlRoot = this.XamlRoot;
                await successDialog.ShowAsync();

                // Refresh the product list
                await ViewModel.LoadItemsAsync();
            }
        }
        catch (Exception ex)
        {
            var failDialog = new ConfirmationDialog("Bulk Import Error", $"Failed to import products: {ex.Message}");
            failDialog.XamlRoot = this.XamlRoot;
            await failDialog.ShowAsync();
        }
    }

    private void OnItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is SportItemListRow row)
            Frame.Navigate(typeof(SportItemDetailPage), row.Item);
    }
}

