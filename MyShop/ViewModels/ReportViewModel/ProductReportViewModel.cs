using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models.ReportModels;
using MyShop.Services;

namespace MyShop.ViewModels.ReportViewModel;

public partial class ProductReportViewModel : ObservableObject
{
  private readonly ReportService _service;
  private CancellationTokenSource? _reloadCts;

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

  public string EndDateDisplay => EndDateValue.ToString("MMM dd, yyyy");
  public string TotalRevenueDisplay => $"{TotalRevenue:N0} VND";
  public string TotalQuantitySoldDisplay => $"{TotalQuantitySold:N0}";
  public string TotalProfitDisplay => $"{TotalProfit:N0} VND";
  public string TotalCustomersDisplay => $"{TotalCustomers:N0}";
  public bool IsDaySelected => IsPeriodSelected("day");
  public bool IsWeekSelected => IsPeriodSelected("week");
  public bool IsMonthSelected => IsPeriodSelected("month");
  public bool IsYearSelected => IsPeriodSelected("year");
  public string DayButtonBackground => IsDaySelected ? "#8B5CF6" : "#F3F4F6";
  public string WeekButtonBackground => IsWeekSelected ? "#8B5CF6" : "#F3F4F6";
  public string MonthButtonBackground => IsMonthSelected ? "#8B5CF6" : "#F3F4F6";
  public string YearButtonBackground => IsYearSelected ? "#8B5CF6" : "#F3F4F6";
  public string DayButtonForeground => IsDaySelected ? "#FFFFFF" : "#6B7280";
  public string WeekButtonForeground => IsWeekSelected ? "#FFFFFF" : "#6B7280";
  public string MonthButtonForeground => IsMonthSelected ? "#FFFFFF" : "#6B7280";
  public string YearButtonForeground => IsYearSelected ? "#FFFFFF" : "#6B7280";

  [ObservableProperty]
  private List<ProductSale> _sales = [];

  [ObservableProperty]
  private decimal _totalRevenue;

  [ObservableProperty]
  private int _totalQuantitySold;

  [ObservableProperty]
  private decimal _totalProfit;

  [ObservableProperty]
  private int _totalCustomers;

  [ObservableProperty]
  private List<RevenueData> _categoryRevenue = [];

  [ObservableProperty]
  private List<ProfitData> _categoryProfit = [];

  [ObservableProperty]
  private List<TopPerformingProduct> _topPerformingProducts = [];

  [ObservableProperty]
  private List<string> _topPerformingProductLines = [];

  [ObservableProperty]
  private List<string> _salesLines = [];

  [ObservableProperty]
  private List<string> _categoryRevenueLines = [];

  [ObservableProperty]
  private List<string> _categoryProfitLines = [];

  [RelayCommand]
  public async Task LoadReportAsync()
  {
    try
    {
      IsLoading = true;
      ErrorMessage = string.Empty;
      Output = "Dang tai report...";

      var totalRevenueTask = _service.GetTotalRevenueAsync(Filter);
      var totalQuantitySoldTask = _service.GetTotalQuantitySoldAsync(Filter);
      var totalProfitTask = _service.GetTotalProfitAsync(Filter);
      var totalCustomersTask = _service.GetTotalCustomersAsync(Filter);
      var salesTask = _service.GetProductSalesInPeriodAsync(Filter);
      var categoryRevenueTask = _service.GetCategoryRevenueAsync(Filter);
      var categoryProfitTask = _service.GetCurrentCategoryProfitAsync(Filter.Period);
      var topPerformingProductsTask = _service.GetTopPerformingProductsAsync(Filter);

      await Task.WhenAll(
        totalRevenueTask,
        totalQuantitySoldTask,
        totalProfitTask,
        totalCustomersTask,
        salesTask,
        categoryRevenueTask,
        categoryProfitTask,
        topPerformingProductsTask);

      TotalRevenue = totalRevenueTask.Result;
      TotalQuantitySold = totalQuantitySoldTask.Result;
      TotalProfit = totalProfitTask.Result;
      TotalCustomers = totalCustomersTask.Result;
      OnPropertyChanged(nameof(TotalRevenueDisplay));
      OnPropertyChanged(nameof(TotalQuantitySoldDisplay));
      OnPropertyChanged(nameof(TotalProfitDisplay));
      OnPropertyChanged(nameof(TotalCustomersDisplay));
      Sales = salesTask.Result;
      CategoryRevenue = categoryRevenueTask.Result;
      CategoryProfit = categoryProfitTask.Result;
      TopPerformingProducts = topPerformingProductsTask.Result;
      TopPerformingProductLines = TopPerformingProducts.Count == 0
        ? ["- No data"]
        : TopPerformingProducts.Select(product =>
          $"{product.ProductName} | Category: {product.CategoryName} | Qty: {product.TotalQuantitySold} | Revenue: {product.GrossRevenue:N2} | Profit: {product.Profit:N2} | Images: {string.Join(", ", product.ImageUrls)}").ToList();
      SalesLines = Sales.Count == 0
        ? ["- No data"]
        : Sales.Select(sale =>
          $"{sale.GetPeriod()} | Qty: {sale.QuantitySold} | Revenue: {sale.GrossRevenue:N2}").ToList();
      CategoryRevenueLines = CategoryRevenue.Count == 0
        ? ["- No data"]
        : CategoryRevenue.Select(item =>
          $"{item.CategoryName} | Revenue: {item.GrossRevenue:N2}").ToList();
      CategoryProfitLines = CategoryProfit.Count == 0
        ? ["- No data"]
        : CategoryProfit.Select(item =>
          $"{item.CategoryName} | Profit: {item.Profit:N2}").ToList();

      var lines = new List<string>
      {
        $"Period: {Filter.Period}",
        $"Start: {Filter.StartDate:yyyy-MM-dd}",
        $"End: {Filter.EndDate:yyyy-MM-dd}",
        $"Category: {(string.IsNullOrWhiteSpace(Filter.CategoryName) ? "All" : Filter.CategoryName)}",
        $"Product: {(string.IsNullOrWhiteSpace(Filter.ProductName) ? "All" : Filter.ProductName)}",
        $"Overview revenue: {TotalRevenue:N2}",
        $"Overview quantity sold: {TotalQuantitySold}",
        $"Overview profit: {TotalProfit:N2}",
        $"Overview customers: {TotalCustomers}",
        $"Sales count: {Sales.Count}",
        $"Category revenue count: {CategoryRevenue.Count}",
        $"Category profit count: {CategoryProfit.Count}",
        $"Top products count: {TopPerformingProducts.Count}",
        "",
        "Revenue by category:",
      };

      if (CategoryRevenue.Count == 0)
      {
        lines.Add("- No data");
      }
      else
      {
        lines.AddRange(CategoryRevenueLines.Select(line => $"- {line}"));
      }

      lines.AddRange(
      [
        "",
        "Profit by category:"
      ]);

      if (CategoryProfit.Count == 0)
      {
        lines.Add("- No data");
      }
      else
      {
        lines.AddRange(CategoryProfitLines.Select(line => $"- {line}"));
      }

      lines.AddRange(
      [
        "",
        "Top performing products:",
      ]);

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
        "",
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

  public void UpdateProductName(string? value)
  {
    Filter.ProductName = string.IsNullOrWhiteSpace(value) ? null : value;
    ScheduleReload();
  }

  public void UpdateCategoryName(string? value)
  {
    Filter.CategoryName = string.IsNullOrWhiteSpace(value) ? null : value;
    ScheduleReload();
  }

  public void UpdateStartDate(DateTime date)
  {
    Filter.StartDate = date.Date;
    OnPropertyChanged(nameof(StartDateValue));
    ScheduleReload();
  }

  public void UpdateEndDate(DateTime date)
  {
    Filter.EndDate = date.Date;
    OnPropertyChanged(nameof(EndDateValue));
    OnPropertyChanged(nameof(EndDateDisplay));
    ScheduleReload();
  }

  public void SetPeriod(string period)
  {
    if (string.Equals(Filter.Period, period, StringComparison.OrdinalIgnoreCase))
    {
      return;
    }

    Filter.Period = period;
    NotifyPeriodSelectionChanged();
    ScheduleReload();
  }

  private bool IsPeriodSelected(string period)
    => string.Equals(Filter.Period, period, StringComparison.OrdinalIgnoreCase);

  private void NotifyPeriodSelectionChanged()
  {
    OnPropertyChanged(nameof(IsDaySelected));
    OnPropertyChanged(nameof(IsWeekSelected));
    OnPropertyChanged(nameof(IsMonthSelected));
    OnPropertyChanged(nameof(IsYearSelected));
    OnPropertyChanged(nameof(DayButtonBackground));
    OnPropertyChanged(nameof(WeekButtonBackground));
    OnPropertyChanged(nameof(MonthButtonBackground));
    OnPropertyChanged(nameof(YearButtonBackground));
    OnPropertyChanged(nameof(DayButtonForeground));
    OnPropertyChanged(nameof(WeekButtonForeground));
    OnPropertyChanged(nameof(MonthButtonForeground));
    OnPropertyChanged(nameof(YearButtonForeground));
  }

  private void ScheduleReload()
  {
    _reloadCts?.Cancel();
    _reloadCts = new CancellationTokenSource();
    _ = ReloadAfterDelayAsync(_reloadCts.Token);
  }

  private async Task ReloadAfterDelayAsync(CancellationToken cancellationToken)
  {
    try
    {
      await Task.Delay(350, cancellationToken);
      if (!cancellationToken.IsCancellationRequested)
      {
        await LoadReportAsync();
      }
    }
    catch (TaskCanceledException)
    {
    }
  }

  partial void OnFilterChanged(ProductSalesFilter value)
  {
    OnPropertyChanged(nameof(StartDateValue));
    OnPropertyChanged(nameof(EndDateValue));
    OnPropertyChanged(nameof(EndDateDisplay));
    NotifyPeriodSelectionChanged();
  }
}
