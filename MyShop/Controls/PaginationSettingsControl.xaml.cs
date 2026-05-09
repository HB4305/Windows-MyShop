using System.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Controls;

public sealed partial class PaginationSettingsControl : UserControl
{
    public PaginationSettingsControl()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(PaginationSettingsControl), new PropertyMetadata(null));

    public static readonly DependencyProperty SelectedItemsPerPageProperty =
        DependencyProperty.Register(nameof(SelectedItemsPerPage), typeof(int), typeof(PaginationSettingsControl), new PropertyMetadata(5));

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public int SelectedItemsPerPage
    {
        get => (int)GetValue(SelectedItemsPerPageProperty);
        set => SetValue(SelectedItemsPerPageProperty, value);
    }
}
