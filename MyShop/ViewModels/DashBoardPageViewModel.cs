using CommunityToolkit.Mvvm.ComponentModel;
using MyShop.ViewModels.Controls;
using System.Threading.Tasks;

namespace MyShop.ViewModels;

public partial class DashBoardPageViewModel : ObservableObject
{
    public SaleMonthlyChartViewModel SaleMonthlyVM { get; }

    public DashBoardPageViewModel(SaleMonthlyChartViewModel saleMonthlyVM)
    {
        SaleMonthlyVM = saleMonthlyVM;
        LoadInitialData();
    }

    private async void LoadInitialData()
    {
        try
        {
            await LoadDataAsync();
        }
        catch (System.Exception ex)
        {
            // Ignore UI break down if data fails to fetch
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
    }

    public async Task LoadDataAsync()
    {
        await SaleMonthlyVM.LoadDataAsync();
    }
}
