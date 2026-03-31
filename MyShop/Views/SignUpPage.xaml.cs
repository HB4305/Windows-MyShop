using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;

namespace MyShop.Views;

public sealed partial class SignUpPage : Page
{
    public SignUpPage()
    {
        this.InitializeComponent();
        DataContext = App.Services.GetRequiredService<LoginViewModel>();
    }
}
