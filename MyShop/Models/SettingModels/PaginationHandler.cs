namespace MyShop.Models.SettingModels;

public class PaginationHandler
{
  public int CurrentPage { get; set; } = 1;
  public int ItemsPerPage { get; set; } = 5;

  public int TotalItems { get; set; }
  public int TotalPages => ItemsPerPage <= 0
    ? 0
    : (int)Math.Ceiling((double)TotalItems / ItemsPerPage);

  public int Skip => ItemsPerPage <= 0 ? 0 : (CurrentPage - 1) * ItemsPerPage;
}
