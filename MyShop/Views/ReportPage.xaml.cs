using Microsoft.Extensions.DependencyInjection;
using MyShop.ViewModels.ReportViewModel;

namespace MyShop.Views;

public sealed partial class ReportPage : Page
{
  public ReportPage()
  {
    this.InitializeComponent();
    DataContext = App.Services.GetRequiredService<ProductReportViewModel>();
  }
}
