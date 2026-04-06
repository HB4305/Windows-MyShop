using Microsoft.Extensions.DependencyInjection;
using MyShop.ViewModels;
using MyShop.Views.Dialogs;

namespace MyShop.Views;

public sealed partial class SettingsPage : Page
{
  public SettingsPage()
  {
    this.InitializeComponent();
    DataContext = App.Services.GetRequiredService<SettingsViewModel>();
  }

  private async void SaveChanges_Click(object sender, RoutedEventArgs e)
  {
    if (DataContext is SettingsViewModel viewModel)
    {
      viewModel.SaveChangesCommand.Execute(null);
      var dialog = new SuccessDialog("Thành công", "Đã lưu cài đặt.");
      dialog.XamlRoot = XamlRoot;
      await dialog.ShowAsync();
    }
  }
}
