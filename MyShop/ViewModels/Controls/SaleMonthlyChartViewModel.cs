using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using MyShop.Services;
using MyShop.Models.ControlModels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.ViewModels.Controls;

public partial class SaleMonthlyChartViewModel : ObservableObject
{
    private readonly CustomerOrderService _customerOrderService;

    [ObservableProperty]
    private ObservableCollection<ISeries> _series;
    
    [ObservableProperty]
    private Axis[] _xAxes = new Axis[0];

    [ObservableProperty]
    private Axis[] _yAxes = new Axis[0];

    [ObservableProperty]
    private ObservableCollection<SaleMonthlyChart> _chartData;

    public SaleMonthlyChartViewModel(CustomerOrderService customerOrderService)
    {
        _customerOrderService = customerOrderService;
    }

    public async Task LoadDataAsync()
    {
        var data = await _customerOrderService.GetSaleMonthlyChartAsync();
        ChartData = new ObservableCollection<SaleMonthlyChart>(data);

        // Map colors to match the provided interface
        var strokeColor = SKColor.Parse("#8b5cf6"); // Purple
        var fillColor = SKColor.Parse("#ddd6fe"); // Light purple gradient approximation

        Series = new ObservableCollection<ISeries>
        {
            new LineSeries<decimal>
            {
                Name = "Revenue ($)",
                Values = new ObservableCollection<decimal>(data.Select(d => d.Revenue)),
                Fill = new LinearGradientPaint(new[] { fillColor, SKColors.White.WithAlpha(50) }, new SKPoint(0.5f, 0), new SKPoint(0.5f, 1)),
                Stroke = new SolidColorPaint(strokeColor) { StrokeThickness = 3 },
                GeometryFill = null,
                GeometryStroke = null,
                LineSmoothness = 0 // Sharp lines like in the picture
            }
        };

        XAxes = new Axis[]
        {
            new Axis
            {
                Labels = data.Select(d => $"DAY {d.Date.Day}").ToArray(),
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#9ca3af")), // Gray
                TextSize = 12,
                MinStep = 1,
                ForceStepToMin = false,
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#f3f4f6")) { StrokeThickness = 1, PathEffect = new DashEffect(new float[] { 3, 3 }) }
            }
        };

        YAxes = new Axis[]
        {
            new Axis
            {
                IsVisible = true,
                MinLimit = 0, // Ensure Y axis starts at 0
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#9ca3af")), // Gray
                TextSize = 12,
                Labeler = value => $"${value:N0}", // Format with $ and thousands separator
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#f3f4f6")) { StrokeThickness = 1, PathEffect = new DashEffect(new float[] { 3, 3 }) }
            }
        };
    }
}
