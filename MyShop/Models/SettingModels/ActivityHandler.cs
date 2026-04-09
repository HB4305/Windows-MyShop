namespace MyShop.Models.SettingModels;

public class ActivityHandler
{
  public bool RememberLastActivity { get; set; } = true;
  public string? LastActivity { get; set; }
}
