namespace MyShop.Models.SettingModels;

public class ActivityHandler
{
  public bool RememberLastActivity { get; set; } = false;
  public string? LastActivity { get; set; }
}
