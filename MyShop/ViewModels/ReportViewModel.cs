using System.Globalization;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using MyShop.Models;
using MyShop.Models.ReportModels;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class ReportViewModel : ObservableObject
{
  private readonly ReportService _reportService;
  private readonly CategoryService _categoryService;
  private readonly SportItemService _sportItemService;
  private readonly Dictionary<string, int> _categoryIdsByName = new(StringComparer.OrdinalIgnoreCase);
  private List<string> _availableProductOptions = [];

  private static Brush SelectedButtonBrush => new SolidColorBrush(ColorHelper.FromArgb(255, 139, 82, 255));

  private static Brush UnselectedButtonBrush =>
    Application.Current.Resources["ControlFillColorDefaultBrush"] as Brush ?? new SolidColorBrush(Colors.Transparent);

  private static Brush SelectedButtonForegroundBrush => new SolidColorBrush(Colors.White);

  private static Brush UnselectedButtonForegroundBrush =>
    Application.Current.Resources["TextFillColorPrimary"] as Brush ?? new SolidColorBrush(ColorHelper.FromArgb(255, 31, 41, 55));

  public ReportViewModel(
    ReportService reportService,
    CategoryService categoryService,
    SportItemService sportItemService)
  {
    _reportService = reportService;
    _categoryService = categoryService;
    _sportItemService = sportItemService;

    ProductSalesFilter = new ProductSalesFilter
    {
      Period = ReportPeriod.Week
    };

    PeriodSelection = ReportService.CreatePeriodSelection(ReportPeriod.Week);
    Overview = new ReportOverview();
    SoldQuantityDatas = [];
    RevenueDatas = [];
    ProfitDatas = [];
    TopPerformingProducts = [];
    CategoryOptions = ["All"];
    ProductOptions = [];

    _ = InitializeAsync();
  }

  [ObservableProperty] private bool _isLoading;
  [ObservableProperty] private string _errorMessage = string.Empty;
  [ObservableProperty] private ReportOverview _overview;
  [ObservableProperty] private ProductSalesFilter _productSalesFilter;
  [ObservableProperty] private PeriodSelection _periodSelection;
  [ObservableProperty] private List<SoldQuantityData> _soldQuantityDatas;
  [ObservableProperty] private List<RevenueData> _revenueDatas;
  [ObservableProperty] private List<ProfitByCategory> _profitDatas;
  [ObservableProperty] private List<TopPerformingProduct> _topPerformingProducts;
  [ObservableProperty] private List<string> _categoryOptions;
  [ObservableProperty] private List<string> _productOptions;
  [ObservableProperty] private string _selectedCategory = "All";
  [ObservableProperty] private string _productSearchText = string.Empty;
  [ObservableProperty] private ISeries[] _soldQuantitySeries = [];
  [ObservableProperty] private Axis[] _soldQuantityXAxes = [];
  [ObservableProperty] private Axis[] _soldQuantityYAxes = [];
  [ObservableProperty] private ISeries[] _revenueSeries = [];
  [ObservableProperty] private Axis[] _revenueXAxes = [];
  [ObservableProperty] private Axis[] _revenueYAxes = [];
  [ObservableProperty] private ISeries[] _profitSeries = [];

  public DateTime RangeStartDisplay => PeriodSelection.Range.Start;

  public DateTime RangeEndDisplay => PeriodSelection.Range.End.AddDays(-1);

  public string PeriodTitle => PeriodSelection.Period switch
  {
    ReportPeriod.Week => "Previous Week",
    ReportPeriod.Month => "Previous Month",
    ReportPeriod.Year => "Previous Year",
    _ => "Selected Period"
  };

  public Brush WeekButtonBackground => GetPeriodButtonBackground(ReportPeriod.Week);

  public Brush MonthButtonBackground => GetPeriodButtonBackground(ReportPeriod.Month);

  public Brush YearButtonBackground => GetPeriodButtonBackground(ReportPeriod.Year);

  public Brush WeekButtonForeground => GetPeriodButtonForeground(ReportPeriod.Week);

  public Brush MonthButtonForeground => GetPeriodButtonForeground(ReportPeriod.Month);

  public Brush YearButtonForeground => GetPeriodButtonForeground(ReportPeriod.Year);

  private async Task InitializeAsync()
  {
    await LoadCategoriesAsync();
    await LoadProductOptionsAsync();
    await LoadReportAsync();
  }

  [RelayCommand]
  private async Task LoadReportAsync()
  {
    try
    {
      IsLoading = true;
      ErrorMessage = string.Empty;

      Overview = await _reportService.GetReportOverviewAsync(PeriodSelection);
      SoldQuantityDatas = await _reportService.GetProductSalesAsync(ProductSalesFilter, PeriodSelection);
      RevenueDatas = await _reportService.GetRevenueDataAsync(PeriodSelection);
      ProfitDatas = await _reportService.GetProfitDataAsync(PeriodSelection);
      TopPerformingProducts = await _reportService.GetTopPerformingProductsAsync(PeriodSelection);
      RefreshCharts();
    }
    catch (Exception ex)
    {
      ErrorMessage = $"Failed to load report: {ex.Message}";
      SoldQuantitySeries = [];
      RevenueSeries = [];
      ProfitSeries = [];
      SoldQuantityXAxes = [];
      SoldQuantityYAxes = [];
      RevenueXAxes = [];
      RevenueYAxes = [];
    }
    finally
    {
      IsLoading = false;
      OnPropertyChanged(nameof(HasProductSales));
      OnPropertyChanged(nameof(HasRevenueData));
      OnPropertyChanged(nameof(HasProfitData));
      OnPropertyChanged(nameof(HasTopProducts));
    }
  }

  public bool HasProductSales => SoldQuantityDatas.Count > 0;

  public bool HasRevenueData => RevenueDatas.Count > 0;

  public bool HasProfitData => ProfitDatas.Count > 0;

  public bool HasTopProducts => TopPerformingProducts.Count > 0;

  public void SetPeriod(ReportPeriod period)
  {
    if (PeriodSelection.Period == period)
    {
      return;
    }

    ProductSalesFilter.Period = period;
    PeriodSelection = ReportService.CreatePeriodSelection(period);
    NotifyPeriodChanged();
    _ = LoadReportAsync();
  }

  public void UpdateProductName(string? value)
  {
    ProductSearchText = value ?? string.Empty;
    ProductSalesFilter.ProductName = NormalizeFilter(ProductSearchText);
    FilterProductOptions(ProductSearchText);
  }

  public void ApplyFilters() => _ = LoadReportAsync();

  public async Task UpdateCategoryAsync(string? value)
  {
    SelectedCategory = string.IsNullOrWhiteSpace(value) ? "All" : value;
    ProductSalesFilter.CategoryName = SelectedCategory == "All" ? null : SelectedCategory;
    ProductSearchText = string.Empty;
    ProductSalesFilter.ProductName = null;

    await LoadProductOptionsAsync();
  }

  private async Task LoadCategoriesAsync()
  {
    try
    {
      var categories = await _categoryService.GetAllAsync();
      _categoryIdsByName.Clear();

      foreach (var category in categories)
      {
        if (!string.IsNullOrWhiteSpace(category.Name))
        {
          _categoryIdsByName[category.Name] = category.Id;
        }
      }

      CategoryOptions = ["All", .. categories
        .Select(category => category.Name)
        .Where(name => !string.IsNullOrWhiteSpace(name))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(name => name)];
    }
    catch
    {
      _categoryIdsByName.Clear();
      CategoryOptions = ["All"];
    }
  }

  private async Task LoadProductOptionsAsync()
  {
    try
    {
      int? categoryId = null;

      if (ProductSalesFilter.CategoryName is not null
        && _categoryIdsByName.TryGetValue(ProductSalesFilter.CategoryName, out var resolvedCategoryId))
      {
        categoryId = resolvedCategoryId;
      }

      _availableProductOptions = await _sportItemService.GetProductNamesAsync(categoryId);
      FilterProductOptions(ProductSearchText);
    }
    catch
    {
      _availableProductOptions = [];
      ProductOptions = [];
    }
  }

  partial void OnPeriodSelectionChanged(PeriodSelection value)
  {
    NotifyPeriodChanged();
  }

  private void NotifyPeriodChanged()
  {
    OnPropertyChanged(nameof(RangeStartDisplay));
    OnPropertyChanged(nameof(RangeEndDisplay));
    OnPropertyChanged(nameof(PeriodTitle));
    OnPropertyChanged(nameof(WeekButtonBackground));
    OnPropertyChanged(nameof(MonthButtonBackground));
    OnPropertyChanged(nameof(YearButtonBackground));
    OnPropertyChanged(nameof(WeekButtonForeground));
    OnPropertyChanged(nameof(MonthButtonForeground));
    OnPropertyChanged(nameof(YearButtonForeground));
  }

  private void RefreshCharts()
  {
    RefreshSoldQuantityChart();
    RefreshRevenueChart();
    RefreshProfitChart();
  }

  private void RefreshSoldQuantityChart()
  {
    var labels = SoldQuantityDatas.Select(point => point.Date.ToString("dd/MM")).ToArray();
    var values = SoldQuantityDatas.Select(point => (double)point.QuantitySold).ToArray();

    SoldQuantitySeries =
    [
      new LineSeries<double>
      {
        Values = values,
        Name = "Units sold"
      }
    ];

    SoldQuantityXAxes =
    [
      new Axis
      {
        Labels = labels,
        Name = "Date",
        MinStep = 1
      }
    ];

    SoldQuantityYAxes =
    [
      new Axis
      {
        Name = "Units sold",
        Labeler = value => Math.Round(value).ToString("N0")
      }
    ];
  }

  private void RefreshRevenueChart()
  {
    var labels = RevenueDatas.Select(point => point.Date.ToString("dd/MM")).ToArray();
    var values = RevenueDatas.Select(point => (double)point.GrossRevenue).ToArray();

    RevenueSeries =
    [
      new ColumnSeries<double>
      {
        Values = values,
        Name = "Revenue"
      }
    ];

    RevenueXAxes =
    [
      new Axis
      {
        Labels = labels,
        Name = "Date",
        MinStep = 1
      }
    ];

    RevenueYAxes =
    [
      new Axis
      {
        Name = "Revenue (USD)",
        Labeler = value => value.ToString("C0", CultureInfo.GetCultureInfo("en-US"))
      }
    ];
  }

  private void RefreshProfitChart()
  {
    double totalProfit = ProfitDatas.Sum(point => (double)point.Profit);

    ProfitSeries = ProfitDatas
      .Select(point => (ISeries)new PieSeries<double>
      {
        Values = [(double)point.Profit],
        Name = totalProfit <= 0
          ? point.CategoryName
          : $"{point.CategoryName} ({((double)point.Profit / totalProfit * 100):0.##}%)"
      })
      .ToArray();
  }

  private Brush GetPeriodButtonBackground(ReportPeriod period)
    => PeriodSelection.Period == period ? SelectedButtonBrush : UnselectedButtonBrush;

  private Brush GetPeriodButtonForeground(ReportPeriod period)
    => PeriodSelection.Period == period ? SelectedButtonForegroundBrush : UnselectedButtonForegroundBrush;

  private static string? NormalizeFilter(string? value)
  {
    var trimmed = value?.Trim();
    return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
  }

  private void FilterProductOptions(string? value)
  {
    var keyword = NormalizeFilter(value);

    ProductOptions = keyword is null
      ? [.. _availableProductOptions]
      : [.. _availableProductOptions.Where(name =>
        name.Contains(keyword, StringComparison.OrdinalIgnoreCase))];
  }
}
