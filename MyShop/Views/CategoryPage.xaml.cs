using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MyShop.Models;
using MyShop.ViewModels;

namespace MyShop.Views;

public sealed partial class CategoryPage : Page
{
    public CategoryViewModel ViewModel { get; }

    public CategoryPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<CategoryViewModel>();
        this.DataContext = ViewModel;

        Loaded += CategoryPage_Loaded;

        ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.CurrentPage) || e.PropertyName == nameof(ViewModel.TotalItems))
            {
                BuildPagination();
            }
        };

        // UI customization for AddEdit Category dialog
        ViewModel.ShowAddEditCategoryFormAsync = async (category) =>
        {
            var dialog = new Forms.AddEditCategoryForm(category) { XamlRoot = this.XamlRoot };
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                return new CategoryViewModel.AddEditCategoryPayload(dialog.CategoryName, dialog.CategoryDescription);
            }
            return null;
        };

        ViewModel.ShowConfirmationDialogAsync = async (title, content) =>
        {
            var dialog = new Dialogs.ConfirmationDialog(title, content) { XamlRoot = this.XamlRoot };
            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        };
    }

    private async void CategoryPage_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadCategoriesAsync();
        BuildPagination();
    }

    private void OnSearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        ViewModel.SearchCommand.Execute(null);
    }

    private void EditCategory_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Category category)
        {
            _ = ViewModel.EditCategoryCommand.ExecuteAsync(category);
        }
    }

    private void DeleteCategory_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Category category)
        {
            _ = ViewModel.DeleteCategoryCommand.ExecuteAsync(category);
        }
    }

    private void BuildPagination()
    {
        if (CategoryPaginationPanel == null) return;
        CategoryPaginationPanel.Children.Clear();

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
                _ = ViewModel.LoadCategoriesAsync(); 
                BuildPagination();
            };

            CategoryPaginationPanel.Children.Add(btn);
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
        prevBtn.Click += (s, e) => { if (current > 1) { ViewModel.CurrentPage--; _ = ViewModel.LoadCategoriesAsync(); BuildPagination(); } };
        CategoryPaginationPanel.Children.Add(prevBtn);

        // Page numbers logic (smart ellipsis)
        if (total <= 7)
        {
            for (int i = 1; i <= total; i++)
                AddPageBtn(i, i == current);
        }
        else
        {
            AddPageBtn(1, current == 1);
            if (current > 3) CategoryPaginationPanel.Children.Add(new TextBlock { Text = "...", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 4, 0), Foreground = (Brush)Resources["TextFillColorSecondaryBrush"] });

            int start = Math.Max(2, current - 1);
            int end = Math.Min(total - 1, current + 1);

            for (int i = start; i <= end; i++)
                AddPageBtn(i, i == current);

            if (current < total - 2) CategoryPaginationPanel.Children.Add(new TextBlock { Text = "...", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 4, 0), Foreground = (Brush)Resources["TextFillColorSecondaryBrush"] });
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
        nextBtn.Click += (s, e) => { if (current < total) { ViewModel.CurrentPage++; _ = ViewModel.LoadCategoriesAsync(); BuildPagination(); } };
        CategoryPaginationPanel.Children.Add(nextBtn);
    }
}

