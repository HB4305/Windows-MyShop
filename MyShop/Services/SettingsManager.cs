using System.Text.Json;
using MyShop.Models.SettingModels;

namespace MyShop.Services;

/// <summary>
/// Manages user configurations for the Settings page.
/// Saves to %APPDATA%\MyShop\settings.json.
/// </summary>
public class SettingsManager
{
  private readonly string _settingsPath;

  public SettingsManager()
  {
    var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    var appFolder = Path.Combine(appData, "MyShop");
    Directory.CreateDirectory(appFolder);
    _settingsPath = Path.Combine(appFolder, "settings.json");
  }

  public SettingConfig LoadSettings()
  {
    if (!File.Exists(_settingsPath))
    {
      return new SettingConfig();
    }

    try
    {
      var json = File.ReadAllText(_settingsPath);
      return JsonSerializer.Deserialize<SettingConfig>(json) ?? new SettingConfig();
    }
    catch
    {
      return new SettingConfig();
    }
  }

  public void SaveSettings(SettingConfig config)
  {
    var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(_settingsPath, json);
  }

  public int GetItemsPerPage()
    => LoadSettings().Pagination.ItemsPerPage;

  public void SetItemsPerPage(int itemsPerPage)
  {
    var config = LoadSettings();
    config.Pagination.ItemsPerPage = itemsPerPage;
    SaveSettings(config);
  }

  public string? GetLastActivity()
    => LoadSettings().Activity.LastActivity;

  public void SetLastActivity(string? lastActivity)
  {
    var config = LoadSettings();
    config.Activity.LastActivity = lastActivity;
    SaveSettings(config);
  }

  public bool GetRememberLastActivity()
    => LoadSettings().Activity.RememberLastActivity;

  public void SetRememberLastActivity(bool remember)
  {
    var config = LoadSettings();
    config.Activity.RememberLastActivity = remember;
    if (!remember)
      config.Activity.LastActivity = null;
    SaveSettings(config);
  }
}
