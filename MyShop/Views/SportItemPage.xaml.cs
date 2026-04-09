using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MyShop.Models;
using MyShop.Services;
using MyShop.ViewModels;
using Windows.System;

namespace MyShop.Views;

public sealed partial class SportItemPage : Page
{
    public SportItemPage()
    {
        this.InitializeComponent();
        DataContext = App.Services.GetRequiredService<SportItemViewModel>();
        _ = ViewModel.LoadItemsAsync();

        Loaded += SportItemPage_OnLoaded;
    }

    private SportItemViewModel ViewModel => (SportItemViewModel)DataContext;

    private void SportItemPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        // Sale không thấy cột giá nhập (import price)
        var currentUser = App.Services.GetRequiredService<CurrentUserService>();
        if (currentUser.IsSale)
        {
            HideImportPriceColumn();
        }
    }

    /// <summary>
    /// Ẩn cột giá nhập trong bảng sản phẩm.
    /// Cột này chỉ visible với owner.
    /// </summary>
    private void HideImportPriceColumn()
    {
        // Tìm GridView/ListView body để thay đổi column width
        var listView = FindChild<ListView>(this);
        if (listView == null) return;

        // Thay đổi ColumnDefinition của header row (Grid bên trong)
        // Header Grid trong XAML định nghĩa 6 cột.
        // Với sale: ẩn cột Price (cột 2 = index 2) bằng cách set Width=0
        // Nhưng cách tốt nhất là dùng Converter trên binding.
        // Tạm thời: không làm gì ở đây vì giá nhập KHÔNG nằm trong SportItemPage table
        // (table SportItemPage chỉ hiển thị selling price)
        //
        // Nếu cần ẩn selling price với sale: làm tương tự.
        // XAML columns: 0=Product, 1=Category, 2=Price(SELL), 3=Stock, 4=Status, 5=Actions
    }

    /// <summary>
    /// Tìm child element theo type.
    /// </summary>
    private static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
    {
        int childCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T result)
                return result;
            var descendant = FindChild<T>(child);
            if (descendant != null)
                return descendant;
        }
        return default;
    }

    private void OnSortComboSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0 || sender is not ComboBox cb)
            return;
        if (cb.SelectedItem is not ComboBoxItem { Tag: string tag } || string.IsNullOrEmpty(tag))
            return;

        ViewModel.SortField = tag;
        _ = ViewModel.ReloadWithCurrentSortAsync();
    }

    private void OnSearchKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Enter)
            return;
        e.Handled = true;

        // Đảm bảo VM nhận đúng chuỗi đang gõ (một số nền tảng chưa flush binding khi Enter).
        if (sender is TextBox tb)
            ViewModel.SearchKeyword = tb.Text ?? string.Empty;

        ViewModel.SearchCommand.Execute(null);
    }

    private void OnAddItemClick(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(SportItemDetailPage), null);
    }

    private void OnItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is SportItemListRow row)
            Frame.Navigate(typeof(SportItemDetailPage), row.Item);
    }
}
