using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
  private readonly SettingsManager _settingsManager;

  public SettingsViewModel(SettingsManager settingsManager)
  {
    _settingsManager = settingsManager;
    ItemsPerPageOptions = new ObservableCollection<int> { 5, 10, 15, 20 };

    var saved = _settingsManager.GetItemsPerPage();
    SelectedItemsPerPage = ItemsPerPageOptions.Contains(saved) ? saved : 5;

    RememberLastActivity = _settingsManager.GetRememberLastActivity();
  }

  public ObservableCollection<int> ItemsPerPageOptions { get; }

  [ObservableProperty]
  private int _selectedItemsPerPage;

  partial void OnSelectedItemsPerPageChanged(int value) { }

  [ObservableProperty]
  private bool _rememberLastActivity;

  partial void OnRememberLastActivityChanged(bool value) { }

  [RelayCommand]
  private void SaveChanges()
  {
    _settingsManager.SetItemsPerPage(SelectedItemsPerPage);
    _settingsManager.SetRememberLastActivity(RememberLastActivity);
  }

  [RelayCommand]
  private void Reset()
  {
    SelectedItemsPerPage = 5;
    RememberLastActivity = false;
    _settingsManager.SetItemsPerPage(SelectedItemsPerPage);
    _settingsManager.SetRememberLastActivity(RememberLastActivity);
  }
}
