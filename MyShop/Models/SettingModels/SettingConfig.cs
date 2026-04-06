namespace MyShop.Models.SettingModels;

public class SettingConfig
{
  public PaginationHandler Pagination { get; set; } = new PaginationHandler();
  public ActivityHandler Activity { get; set; } = new ActivityHandler();
}
