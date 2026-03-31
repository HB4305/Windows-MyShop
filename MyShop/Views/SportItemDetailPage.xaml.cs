using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Models;
using MyShop.ViewModels;

namespace MyShop.Views;

public sealed partial class SportItemDetailPage : Page
{
    public SportItemDetailViewModel ViewModel { get; }

    public SportItemDetailPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<SportItemDetailViewModel>();
        DataContext = ViewModel;
        
        ViewModel.SaveCompleted += () =>
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var item = e.Parameter as SportItem;
        await ViewModel.InitializeAsync(item);
    }

    private void OnBackClick(object sender, RoutedEventArgs e)
    {
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
