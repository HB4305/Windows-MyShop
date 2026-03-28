using Microsoft.UI.Xaml.Controls;

namespace MyShop.Views;

public sealed partial class ShellPage : Page
{
    public ShellPage()
    {
        this.InitializeComponent();
        NavView.SelectedItem = NavView.MenuItems[0];
        ContentFrame.Navigate(typeof(CategoryPage));
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            var tag = item.Tag?.ToString();
            if (tag == "Category") ContentFrame.Navigate(typeof(CategoryPage));
            else if (tag == "SportItem") ContentFrame.Navigate(typeof(SportItemPage));
        }
    }
}
