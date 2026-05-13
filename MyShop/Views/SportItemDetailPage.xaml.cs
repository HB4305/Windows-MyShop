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
        if (string.IsNullOrWhiteSpace(url))
            return;

        var grid = new Grid { ColumnSpacing = 12, Padding = new Thickness(0, 10, 0, 0) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

        var image = new Image
        {
            Stretch = Microsoft.UI.Xaml.Media.Stretch.Uniform,
            MaxWidth = 850,
            MaxHeight = 650,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var binding = new Microsoft.UI.Xaml.Data.Binding
        {
            Path = new PropertyPath("PreviewImageUrl"),
            Source = ViewModel,
            Converter = (Microsoft.UI.Xaml.Data.IValueConverter)this.Resources["StringToImageConverter"]
        };
        image.SetBinding(Image.SourceProperty, binding);
        Grid.SetColumn(image, 1);
        grid.Children.Add(image);

        var btnStyle = (Style)this.Resources["LocalColorPreservingButtonStyle"];
        var purpleBrush = (Microsoft.UI.Xaml.Media.Brush)this.Resources["SportPurpleSoftBrush"];

        var prevBtn = new Button
        {
            Content = new FontIcon { Glyph = "\uE76B", FontSize = 20 },
            VerticalAlignment = VerticalAlignment.Center,
            Command = ViewModel.SelectPreviousImageCommand,
            Style = btnStyle,
            Width = 48,
            Height = 48,
            CornerRadius = new CornerRadius(24),
            Background = purpleBrush
        };
        Grid.SetColumn(prevBtn, 0);
        grid.Children.Add(prevBtn);

        var nextBtn = new Button
        {
            Content = new FontIcon { Glyph = "\uE76C", FontSize = 20 },
            VerticalAlignment = VerticalAlignment.Center,
            Command = ViewModel.SelectNextImageCommand,
            Style = btnStyle,
            Width = 48,
            Height = 48,
            CornerRadius = new CornerRadius(24),
            Background = purpleBrush
        };
        Grid.SetColumn(nextBtn, 2);
        grid.Children.Add(nextBtn);

        var dialog = new ContentDialog
        {
            Title = "Product Gallery",
            Content = grid,
            CloseButtonText = "Done",
            XamlRoot = XamlRoot,
            MaxWidth = 1000,
            MaxHeight = 850
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
