using System.Collections;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace MyShop.Controls;

public sealed partial class ProductSearchFilterBar : UserControl
{
    public ProductSearchFilterBar()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty CategoryItemsSourceProperty =
        DependencyProperty.Register(nameof(CategoryItemsSource), typeof(IEnumerable), typeof(ProductSearchFilterBar), new PropertyMetadata(null));

    public static readonly DependencyProperty SelectedCategoryProperty =
        DependencyProperty.Register(nameof(SelectedCategory), typeof(object), typeof(ProductSearchFilterBar), new PropertyMetadata(null));

    public static readonly DependencyProperty SearchTextProperty =
        DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(ProductSearchFilterBar), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty SearchItemsSourceProperty =
        DependencyProperty.Register(nameof(SearchItemsSource), typeof(IEnumerable), typeof(ProductSearchFilterBar), new PropertyMetadata(null));

    public static readonly DependencyProperty SearchCommandProperty =
        DependencyProperty.Register(nameof(SearchCommand), typeof(ICommand), typeof(ProductSearchFilterBar), new PropertyMetadata(null));

    public static readonly DependencyProperty SearchPlaceholderTextProperty =
        DependencyProperty.Register(nameof(SearchPlaceholderText), typeof(string), typeof(ProductSearchFilterBar), new PropertyMetadata("Search product name..."));

    public static readonly DependencyProperty CategoryPlaceholderTextProperty =
        DependencyProperty.Register(nameof(CategoryPlaceholderText), typeof(string), typeof(ProductSearchFilterBar), new PropertyMetadata("Category"));

    public IEnumerable? CategoryItemsSource
    {
        get => (IEnumerable?)GetValue(CategoryItemsSourceProperty);
        set => SetValue(CategoryItemsSourceProperty, value);
    }

    public object? SelectedCategory
    {
        get => GetValue(SelectedCategoryProperty);
        set => SetValue(SelectedCategoryProperty, value);
    }

    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public IEnumerable? SearchItemsSource
    {
        get => (IEnumerable?)GetValue(SearchItemsSourceProperty);
        set => SetValue(SearchItemsSourceProperty, value);
    }

    public ICommand? SearchCommand
    {
        get => (ICommand?)GetValue(SearchCommandProperty);
        set => SetValue(SearchCommandProperty, value);
    }

    public string SearchPlaceholderText
    {
        get => (string)GetValue(SearchPlaceholderTextProperty);
        set => SetValue(SearchPlaceholderTextProperty, value);
    }

    public string CategoryPlaceholderText
    {
        get => (string)GetValue(CategoryPlaceholderTextProperty);
        set => SetValue(CategoryPlaceholderTextProperty, value);
    }

    public event TypedEventHandler<ProductSearchFilterBar, AutoSuggestBoxTextChangedEventArgs>? SearchTextChanged;
    public event TypedEventHandler<ProductSearchFilterBar, AutoSuggestBoxSuggestionChosenEventArgs>? SearchSuggestionChosen;
    public event TypedEventHandler<ProductSearchFilterBar, AutoSuggestBoxQuerySubmittedEventArgs>? SearchQuerySubmitted;
    public event TypedEventHandler<ProductSearchFilterBar, SelectionChangedEventArgs>? CategorySelectionChanged;
    public event RoutedEventHandler? SearchClicked;

    public void CloseFlyouts()
    {
        CategoryComboBox.IsDropDownOpen = false;
        SearchAutoSuggestBox.IsSuggestionListOpen = false;
    }

    private void SearchAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        => SearchTextChanged?.Invoke(this, args);

    private void SearchAutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        => SearchSuggestionChosen?.Invoke(this, args);

    private void SearchAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        => SearchQuerySubmitted?.Invoke(this, args);

    private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => CategorySelectionChanged?.Invoke(this, e);

    private void SearchButton_Click(object sender, RoutedEventArgs e)
        => SearchClicked?.Invoke(this, e);
}
