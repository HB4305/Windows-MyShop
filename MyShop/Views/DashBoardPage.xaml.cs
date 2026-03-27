using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;

namespace MyShop.Views;

public sealed partial class DashBoardPage : Page
{
    public DashBoardPage()
    {
        this.InitializeComponent();
        DataContext = App.Services.GetRequiredService<DashBoardPageViewModel>();
    }
}
