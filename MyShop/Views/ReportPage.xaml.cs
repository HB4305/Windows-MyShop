using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MyShop.Controls;
using MyShop.Models.ReportModels;
using MyShop.ViewModels;

namespace MyShop.Views;

public sealed partial class ReportPage : Page
{
  private const double HeaderResponsiveBreakpoint = 800;
  private const double ResponsiveBreakpoint = 800;
  private const double OverviewTwoColumnBreakpoint = 800;
  private const double OverviewOneColumnBreakpoint = 400;

  public ReportPage()
  {
    this.InitializeComponent();
    DataContext = App.Services.GetRequiredService<ReportViewModel>();
    Loaded += OnLoaded;
  }

  private ReportViewModel? ViewModel => DataContext as ReportViewModel;

  private void OnLoaded(object sender, RoutedEventArgs e)
  {
    UpdateHeaderLayout();
    UpdateOverviewLayout();
    UpdateRevenueProfitLayout();
  }

  private void RootLayout_SizeChanged(object sender, SizeChangedEventArgs e)
  {
    UpdateHeaderLayout();
    UpdateOverviewLayout();
    UpdateRevenueProfitLayout();
  }

  private void ProductFilterBar_SearchTextChanged(ProductSearchFilterBar sender, AutoSuggestBoxTextChangedEventArgs args)
  {
    if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
    {
      ViewModel?.UpdateProductName(sender.SearchText);
    }
  }

  private void ProductFilterBar_SearchSuggestionChosen(ProductSearchFilterBar sender, AutoSuggestBoxSuggestionChosenEventArgs args)
  {
    if (args.SelectedItem is string selectedProduct)
    {
      ViewModel?.UpdateProductName(selectedProduct);
    }
  }

  private void ProductFilterBar_SearchQuerySubmitted(ProductSearchFilterBar sender, AutoSuggestBoxQuerySubmittedEventArgs args)
  {
    ViewModel?.UpdateProductName(args.ChosenSuggestion?.ToString() ?? sender.SearchText);
  }

  private void WeekPeriodButton_Click(object sender, RoutedEventArgs e)
    => ViewModel?.SetPeriod(ReportPeriod.Week);

  private void MonthPeriodButton_Click(object sender, RoutedEventArgs e)
    => ViewModel?.SetPeriod(ReportPeriod.Month);

  private void YearPeriodButton_Click(object sender, RoutedEventArgs e)
    => ViewModel?.SetPeriod(ReportPeriod.Year);

  private async void ProductFilterBar_CategorySelectionChanged(ProductSearchFilterBar sender, SelectionChangedEventArgs e)
  {
    if (ViewModel is not null)
    {
      await ViewModel.UpdateCategoryAsync(sender.SelectedCategory?.ToString());
    }

    CloseOpenDropDowns();
    Focus(FocusState.Programmatic);
  }

  private void ProductFilterBar_SearchClicked(object sender, RoutedEventArgs e)
    => ViewModel?.ApplyFilters();

  private void PageScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
  {
    CloseOpenDropDowns();
  }

  private void Page_Tapped(object sender, TappedRoutedEventArgs e)
  {
    if (e.OriginalSource is DependencyObject source
        && !IsInsideFilterControl(source))
    {
      CloseOpenDropDowns();
    }
  }

  private void UpdateRevenueProfitLayout()
  {
    if (RootLayout.ActualWidth < ResponsiveBreakpoint)
    {
      RevenueProfitGrid.ColumnDefinitions.Clear();
      RevenueProfitGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

      RevenueProfitGrid.RowDefinitions.Clear();
      RevenueProfitGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
      RevenueProfitGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

      Grid.SetColumn(RevenueCard, 0);
      Grid.SetRow(RevenueCard, 0);
      Grid.SetColumn(ProfitCard, 0);
      Grid.SetRow(ProfitCard, 1);
    }
    else
    {
      RevenueProfitGrid.ColumnDefinitions.Clear();
      RevenueProfitGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
      RevenueProfitGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

      RevenueProfitGrid.RowDefinitions.Clear();
      RevenueProfitGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

      Grid.SetColumn(RevenueCard, 0);
      Grid.SetRow(RevenueCard, 0);
      Grid.SetColumn(ProfitCard, 1);
      Grid.SetRow(ProfitCard, 0);
    }
  }

  private void UpdateHeaderLayout()
  {
    if (RootLayout.ActualWidth < HeaderResponsiveBreakpoint)
    {
      HeaderGrid.ColumnDefinitions.Clear();
      HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

      HeaderGrid.RowDefinitions.Clear();
      HeaderGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
      HeaderGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

      Grid.SetColumn(HeaderTitlePanel, 0);
      Grid.SetRow(HeaderTitlePanel, 0);
      Grid.SetColumn(HeaderPeriodCard, 0);
      Grid.SetRow(HeaderPeriodCard, 1);
    }
    else
    {
      HeaderGrid.ColumnDefinitions.Clear();
      HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
      HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

      HeaderGrid.RowDefinitions.Clear();
      HeaderGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

      Grid.SetColumn(HeaderTitlePanel, 0);
      Grid.SetRow(HeaderTitlePanel, 0);
      Grid.SetColumn(HeaderPeriodCard, 1);
      Grid.SetRow(HeaderPeriodCard, 0);
    }
  }

  private void UpdateOverviewLayout()
  {
    if (RootLayout.ActualWidth < OverviewOneColumnBreakpoint)
    {
      SetOverviewColumns(1);
      PlaceOverviewCard(OverviewRevenueCard, 0, 0);
      PlaceOverviewCard(OverviewQuantityCard, 1, 0);
      PlaceOverviewCard(OverviewProfitCard, 2, 0);
      PlaceOverviewCard(OverviewCustomersCard, 3, 0);
      return;
    }

    if (RootLayout.ActualWidth < OverviewTwoColumnBreakpoint)
    {
      SetOverviewColumns(2);
      PlaceOverviewCard(OverviewRevenueCard, 0, 0);
      PlaceOverviewCard(OverviewQuantityCard, 0, 1);
      PlaceOverviewCard(OverviewProfitCard, 1, 0);
      PlaceOverviewCard(OverviewCustomersCard, 1, 1);
      return;
    }

    SetOverviewColumns(4);
    PlaceOverviewCard(OverviewRevenueCard, 0, 0);
    PlaceOverviewCard(OverviewQuantityCard, 0, 1);
    PlaceOverviewCard(OverviewProfitCard, 0, 2);
    PlaceOverviewCard(OverviewCustomersCard, 0, 3);
  }

  private void SetOverviewColumns(int columns)
  {
    OverviewGrid.ColumnDefinitions.Clear();
    for (int i = 0; i < columns; i++)
    {
      OverviewGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
    }

    int rows = columns switch
    {
      1 => 4,
      2 => 2,
      _ => 1
    };

    OverviewGrid.RowDefinitions.Clear();
    for (int i = 0; i < rows; i++)
    {
      OverviewGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
    }
  }

  private static void PlaceOverviewCard(FrameworkElement element, int row, int column)
  {
    Grid.SetRow(element, row);
    Grid.SetColumn(element, column);
  }

  private void CloseOpenDropDowns()
  {
    ProductFilterBar.CloseFlyouts();
  }

  private bool IsInsideFilterControl(DependencyObject source)
  {
    DependencyObject? current = source;
    while (current is not null)
    {
      if (ReferenceEquals(current, ProductFilterBar))
      {
        return true;
      }

      current = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(current);
    }

    return false;
  }
}
