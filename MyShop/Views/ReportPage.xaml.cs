using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels.ReportViewModel;

namespace MyShop.Views;

public sealed partial class ReportPage : Page
{
  public ReportPage()
  {
    this.InitializeComponent();
    DataContext = App.Services.GetRequiredService<ProductReportViewModel>();
  }

  private ProductReportViewModel? ViewModel => DataContext as ProductReportViewModel;

  private void ProductSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
  {
    if (sender is TextBox textBox)
    {
      ViewModel?.UpdateProductName(textBox.Text);
    }
  }

  private void CategoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
  {
    if (sender is TextBox textBox)
    {
      ViewModel?.UpdateCategoryName(textBox.Text);
    }
  }

  private void StartDatePicker_DateChanged(object sender, DatePickerValueChangedEventArgs args)
    => ViewModel?.UpdateStartDate(args.NewDate.Date);

  private void EndDatePicker_DateChanged(object sender, DatePickerValueChangedEventArgs args)
    => ViewModel?.UpdateEndDate(args.NewDate.Date);

  private void DayPeriodButton_Click(object sender, RoutedEventArgs e)
    => ViewModel?.SetPeriod("day");

  private void WeekPeriodButton_Click(object sender, RoutedEventArgs e)
    => ViewModel?.SetPeriod("week");

  private void MonthPeriodButton_Click(object sender, RoutedEventArgs e)
    => ViewModel?.SetPeriod("month");

  private void YearPeriodButton_Click(object sender, RoutedEventArgs e)
    => ViewModel?.SetPeriod("year");
}
