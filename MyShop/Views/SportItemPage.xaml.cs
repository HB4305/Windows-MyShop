using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MyShop.Models;
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
    }

    private SportItemViewModel ViewModel => (SportItemViewModel)DataContext;

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
