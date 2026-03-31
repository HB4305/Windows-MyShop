using Microsoft.Extensions.DependencyInjection;
using MyShop.Models;
using MyShop.ViewModels;
using MyShop.Views.Forms;
using MyShop.Views.Dialogs;

namespace MyShop.Views;

public sealed partial class CategoryPage : Page
{
    private readonly CategoryViewModel _viewModel;

    public CategoryPage()
    {
        this.InitializeComponent();
        _viewModel = App.Services.GetRequiredService<CategoryViewModel>();
        _viewModel.ShowAddEditCategoryFormAsync = ShowAddEditCategoryFormAsync;
        _viewModel.ShowConfirmationDialogAsync = ShowConfirmationDialogAsync;
        DataContext = _viewModel;
    }

    private async Task<bool> ShowConfirmationDialogAsync(string title, string content)
    {
        var dialog = new ConfirmationDialog(title, content)
        {
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    private async Task<CategoryViewModel.AddEditCategoryPayload?> ShowAddEditCategoryFormAsync(Category? category)
    {
        var dialog = new AddEditCategoryForm(category)
        {
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return null;
        }

        return new CategoryViewModel.AddEditCategoryPayload(dialog.NormalizedName, dialog.NormalizedDescription);
    }

    private void EditCategory_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.DataContext is Category category)
        {
            _viewModel.EditCategoryCommand.Execute(category);
        }
    }

    private void DeleteCategory_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.DataContext is Category category)
        {
            _viewModel.DeleteCategoryCommand.Execute(category);
        }
    }
}
