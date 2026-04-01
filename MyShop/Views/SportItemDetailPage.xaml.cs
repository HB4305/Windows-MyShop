using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Models;
using MyShop.ViewModels;
using MyShop.Views.Dialogs;

namespace MyShop.Views;

public sealed partial class SportItemDetailPage : Page
{
    public SportItemDetailViewModel ViewModel { get; }

    public SportItemDetailPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<SportItemDetailViewModel>();
        ViewModel.ShowConfirmationDialogAsync = ShowConfirmationDialogAsync;
        DataContext = ViewModel;

        ViewModel.SaveCompleted += () =>
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        };
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

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var item = e.Parameter as SportItem;
        await ViewModel.InitializeAsync(item);
    }

    private void OnDiscardClick(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
            Frame.GoBack();
    }

    private void OnDeleteImageClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string url)
        {
            ViewModel.DeleteImageCommand.Execute(url);
        }
    }
}
