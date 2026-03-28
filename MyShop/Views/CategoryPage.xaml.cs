using Microsoft.Extensions.DependencyInjection;
using MyShop.ViewModels;

namespace MyShop.Views;

public sealed partial class CategoryPage : Page
{
    public CategoryPage()
    {
        this.InitializeComponent();
        var vm = App.Services.GetRequiredService<CategoryViewModel>();
        DataContext = vm;
        _ = vm.LoadCategoriesAsync();
    }
}
