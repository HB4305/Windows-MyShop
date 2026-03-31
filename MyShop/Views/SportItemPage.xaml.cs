using System.Linq;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Models;
using MyShop.ViewModels;

namespace MyShop.Views;

public sealed partial class SportItemPage : Page
{
    public SportItemPage()
    {
        this.InitializeComponent();
        DataContext = App.Services.GetRequiredService<SportItemViewModel>();
        _ = ViewModel.LoadItemsAsync();
    }

    private void OnPriceTextChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = (TextBox)sender;
        var text = textBox.Text;

        // Chỉ lấy các chữ số
        var rawDigits = new string(text.Where(char.IsDigit).ToArray());

        if (string.IsNullOrEmpty(rawDigits))
        {
            if (textBox == MinPriceBox) ViewModel.MinPrice = null;
            else ViewModel.MaxPrice = null;
            if (text != "") textBox.Text = "";
            return;
        }

        if (long.TryParse(rawDigits, out long value))
        {
            // Cập nhật giá trị vào ViewModel
            if (textBox == MinPriceBox) ViewModel.MinPrice = value;
            else ViewModel.MaxPrice = value;

            // Định dạng với dấu chấm phân cách phần nghìn
            var formatted = value.ToString("N0", new CultureInfo("vi-VN"));

            // Tránh lặp vô hạn và giữ con trỏ ở cuối
            if (text != formatted)
            {
                textBox.Text = formatted;
                textBox.SelectionStart = formatted.Length;
            }
        }
    }

    private SportItemViewModel ViewModel => (SportItemViewModel)DataContext;

    private void OnAddItemClick(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(SportItemDetailPage), null);
    }

    private void OnItemClick(object sender, ItemClickEventArgs e)
    {
        var item = e.ClickedItem as SportItem;
        Frame.Navigate(typeof(SportItemDetailPage), item);
    }
}
