using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;

namespace MyShop.Views;

public sealed partial class ConfigPage : Page
{
    public ConfigPage()
    {
        this.InitializeComponent();
        DataContext = App.Services.GetRequiredService<ConfigViewModel>();
    }
}
