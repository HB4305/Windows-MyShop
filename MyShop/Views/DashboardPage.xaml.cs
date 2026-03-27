using Microsoft.Extensions.DependencyInjection;
using MyShop.ViewModels;

namespace MyShop.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        this.InitializeComponent();
        DataContext = App.Services.GetRequiredService<DashboardViewModel>();
    }
}
