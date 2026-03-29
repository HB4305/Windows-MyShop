using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models.ReportModels;
using MyShop.Services;

namespace MyShop.ViewModels.ReportViewModel;

public partial class ProductReportViewModel : ObservableObject
{
  private readonly ReportService _service;

  public ProductReportViewModel(ReportService service)
  {
    _service = service;
    _ = LoadReportAsync();
  }

  [ObservableProperty]
  private bool _isLoading;

  [ObservableProperty]
  private string _errorMessage = string.Empty;

  [ObservableProperty]
  private string _output = "Dang tai report...";

  [ObservableProperty]
  private ProductSalesFilter _filter = new();

  public DateTimeOffset StartDateValue
  {
    get => new(Filter.StartDate);
    set
    {
      Filter.StartDate = value.Date;
      OnPropertyChanged();
    }
  }

  public DateTimeOffset EndDateValue
  {
    get => new(Filter.EndDate);
    set
    {
      Filter.EndDate = value.Date;
      OnPropertyChanged();
    }
  }

  [ObservableProperty]
  private List<ProductSale> _sales = [];

  [ObservableProperty]
  private List<TopPerformingProduct> _topPerformingProducts = [];

  [ObservableProperty]
  private List<string> _topPerformingProductLines = [];

  [ObservableProperty]
  private List<string> _salesLines = [];

  [RelayCommand]
  public async Task LoadReportAsync()
  {
    try
    {
      IsLoading = true;
      ErrorMessage = string.Empty;
      Output = "Dang tai report...";

      var salesTask = _service.GetProductSalesInPeriodAsync(Filter);
      var topPerformingProductsTask = _service.GetTopPerformingProductsAsync(Filter);

      await Task.WhenAll(salesTask, topPerformingProductsTask);

      Sales = salesTask.Result;
      TopPerformingProducts = topPerformingProductsTask.Result;
      TopPerformingProductLines = TopPerformingProducts.Count == 0
        ? ["- No data"]
        : TopPerformingProducts.Select(product =>
          $"{product.ProductName} | Category: {product.CategoryName} | Qty: {product.TotalQuantitySold} | Revenue: {product.GrossRevenue:N2} | Profit: {product.Profit:N2} | Images: {string.Join(", ", product.ImageUrls)}").ToList();
      SalesLines = Sales.Count == 0
        ? ["- No data"]
        : Sales.Select(sale =>
          $"{sale.GetPeriod()} | Qty: {sale.QuantitySold} | Revenue: {sale.GrossRevenue:N2}").ToList();

      var lines = new List<string>
      {
        $"Period: {Filter.Period}",
        $"Start: {Filter.StartDate:yyyy-MM-dd}",
        $"End: {Filter.EndDate:yyyy-MM-dd}",
        $"Category: {(string.IsNullOrWhiteSpace(Filter.CategoryName) ? "All" : Filter.CategoryName)}",
        $"Product: {(string.IsNullOrWhiteSpace(Filter.ProductName) ? "All" : Filter.ProductName)}",
        $"Sales count: {Sales.Count}",
        $"Top products count: {TopPerformingProducts.Count}",
        "",
        "Top performing products:",
      };

      if (Sales.Count > 0)
      {
        var firstSale = Sales[0];
        lines.Add($"First sale type: {firstSale.GetType().Name}");
        lines.Add($"First sale period: {firstSale.GetPeriod()}");
        lines.Add($"First sale qty: {firstSale.QuantitySold}");
        lines.Add($"First sale revenue: {firstSale.GrossRevenue:N2}");
        lines.Add("");
      }

      if (TopPerformingProducts.Count == 0)
      {
        lines.Add("- No data");
      }
      else
      {
        lines.AddRange(TopPerformingProductLines.Select(line => $"- {line}"));
      }

      lines.AddRange(
      [
        "Sales:"
      ]);

      if (Sales.Count == 0)
      {
        lines.Add("- No data");
      }
      else
      {
        lines.AddRange(SalesLines.Select(line => $"- {line}"));
      }

      Output = string.Join(Environment.NewLine, lines);
    }
    catch (Exception e)
    {
      ErrorMessage = $"Loi: {e.Message}";
      Output = "Khong tai duoc du lieu report.";
    }
    finally
    {
      IsLoading = false;
    }
  }

  partial void OnFilterChanged(ProductSalesFilter value)
  {
    OnPropertyChanged(nameof(StartDateValue));
    OnPropertyChanged(nameof(EndDateValue));
  }
}
