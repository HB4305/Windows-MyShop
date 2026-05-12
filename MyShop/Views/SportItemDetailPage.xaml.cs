using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Models;
using MyShop.Services;
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

        ViewModel.ViewImageDetailRequested += async (s, url) => await ShowImageDetailAsync(url);
    }

    private async Task ShowImageDetailAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url) || !System.Uri.TryCreate(url, System.UriKind.Absolute, out var uri))
            return;

        var image = new Image
        {
            Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(uri),
            Stretch = Microsoft.UI.Xaml.Media.Stretch.Uniform
        };

        var dialog = new ContentDialog
        {
            Title = "Image Preview",
            Content = image,
            CloseButtonText = "Close",
            XamlRoot = XamlRoot,
            MaxWidth = 1000,
            MaxHeight = 800
        };

        await dialog.ShowAsync();
    }

    private async void OnImageTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        if (ViewModel.PreviewImageUrl != null)
        {
            await ShowImageDetailAsync(ViewModel.PreviewImageUrl);
        }
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

        var currentUser = App.Services.GetRequiredService<CurrentUserService>();
        if (currentUser.IsSale)
        {
            SaveButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;
            MainFormGrid.IsHitTestVisible = false;
            MainFormGrid.Opacity = 0.85;
        }
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
