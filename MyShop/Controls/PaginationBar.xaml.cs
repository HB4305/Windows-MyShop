using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Controls;

public sealed partial class PaginationBar : UserControl
{
    public PaginationBar()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty CurrentPageProperty =
        DependencyProperty.Register(nameof(CurrentPage), typeof(int), typeof(PaginationBar), new PropertyMetadata(1));

    public static readonly DependencyProperty TotalPagesProperty =
        DependencyProperty.Register(nameof(TotalPages), typeof(int), typeof(PaginationBar), new PropertyMetadata(1));

    public static readonly DependencyProperty DisplayFromProperty =
        DependencyProperty.Register(nameof(DisplayFrom), typeof(int), typeof(PaginationBar), new PropertyMetadata(0));

    public static readonly DependencyProperty DisplayToProperty =
        DependencyProperty.Register(nameof(DisplayTo), typeof(int), typeof(PaginationBar), new PropertyMetadata(0));

    public static readonly DependencyProperty TotalItemsProperty =
        DependencyProperty.Register(nameof(TotalItems), typeof(int), typeof(PaginationBar), new PropertyMetadata(0));

    public static readonly DependencyProperty ItemLabelProperty =
        DependencyProperty.Register(nameof(ItemLabel), typeof(string), typeof(PaginationBar), new PropertyMetadata("items"));

    public static readonly DependencyProperty PreviousCommandProperty =
        DependencyProperty.Register(nameof(PreviousCommand), typeof(ICommand), typeof(PaginationBar), new PropertyMetadata(null));

    public static readonly DependencyProperty NextCommandProperty =
        DependencyProperty.Register(nameof(NextCommand), typeof(ICommand), typeof(PaginationBar), new PropertyMetadata(null));

    public int CurrentPage
    {
        get => (int)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public int TotalPages
    {
        get => (int)GetValue(TotalPagesProperty);
        set => SetValue(TotalPagesProperty, value);
    }

    public int DisplayFrom
    {
        get => (int)GetValue(DisplayFromProperty);
        set => SetValue(DisplayFromProperty, value);
    }

    public int DisplayTo
    {
        get => (int)GetValue(DisplayToProperty);
        set => SetValue(DisplayToProperty, value);
    }

    public int TotalItems
    {
        get => (int)GetValue(TotalItemsProperty);
        set => SetValue(TotalItemsProperty, value);
    }

    public string ItemLabel
    {
        get => (string)GetValue(ItemLabelProperty);
        set => SetValue(ItemLabelProperty, value);
    }

    public ICommand? PreviousCommand
    {
        get => (ICommand?)GetValue(PreviousCommandProperty);
        set => SetValue(PreviousCommandProperty, value);
    }

    public ICommand? NextCommand
    {
        get => (ICommand?)GetValue(NextCommandProperty);
        set => SetValue(NextCommandProperty, value);
    }
}
