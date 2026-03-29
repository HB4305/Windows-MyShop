using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using MyShop.Models;
using MyShop.ViewModels;
using WpfFontWeights = Microsoft.UI.Text.FontWeights;
using ChartPath = Microsoft.UI.Xaml.Shapes.Path;

namespace MyShop.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        this.InitializeComponent();
        DataContext = App.Services.GetRequiredService<DashboardViewModel>();
        Loaded += DashboardPage_Loaded;
    }

    private void DashboardPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is DashboardViewModel vm)
        {
            vm.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(vm.DailyRevenuePoints))
                    DrawChart(vm.DailyRevenuePoints);
            };
            if (vm.DailyRevenuePoints.Count > 0)
                DrawChart(vm.DailyRevenuePoints);
        }
    }

    private void RevenueChartCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (DataContext is DashboardViewModel vm && vm.DailyRevenuePoints.Count > 0)
            DrawChart(vm.DailyRevenuePoints);
    }

    private void DrawChart(List<RevenueReport> dataPoints)
    {
        RevenueChartCanvas.Children.Clear();
        if (dataPoints == null || dataPoints.Count == 0) return;

        var canvas = RevenueChartCanvas;
        var w = canvas.ActualWidth;
        var h = canvas.ActualHeight;
        if (w <= 0 || h <= 0) return;

        const double padLeft = 8, padRight = 8, padTop = 10;
        double chartW = w - padLeft - padRight;
        double chartH = h - padTop - 10;

        var maxRevenue = dataPoints.Max(d => d.GrossRevenue);
        if (maxRevenue <= 0) maxRevenue = 1;

        var points = new List<Windows.Foundation.Point>();
        for (int i = 0; i < dataPoints.Count; i++)
        {
            double x = padLeft + (double)i / Math.Max(dataPoints.Count - 1, 1) * chartW;
            double y = padTop + chartH - ((double)dataPoints[i].GrossRevenue / (double)maxRevenue * chartH);
            points.Add(new Windows.Foundation.Point(x, y));
        }

        // ── Area fill (Polygon) ────────────────────────────────
        var areaPoints = new PointCollection();
        areaPoints.Add(new Windows.Foundation.Point(points[0].X, padTop + chartH));
        foreach (var pt in points) areaPoints.Add(pt);
        areaPoints.Add(new Windows.Foundation.Point(points[^1].X, padTop + chartH));

        var areaPolygon = new Polygon
        {
            Points = areaPoints,
            Fill = new SolidColorBrush(Color.FromArgb(50, 124, 58, 237)),
            Stroke = null
        };
        canvas.Children.Add(areaPolygon);

        // ── Line (Polyline for simplicity, no Bezier) ─────────
        var linePoints = new PointCollection();
        foreach (var pt in points) linePoints.Add(pt);

        var line = new Polyline
        {
            Points = linePoints,
            Stroke = new SolidColorBrush(Color.FromArgb(255, 124, 58, 237)),
            StrokeThickness = 2.5
        };
        canvas.Children.Add(line);

        // ── Dots at each point ────────────────────────────────
        foreach (var pt in points)
        {
            var dot = new Ellipse
            {
                Width = 6, Height = 6,
                Fill = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                Stroke = new SolidColorBrush(Color.FromArgb(255, 124, 58, 237)),
                StrokeThickness = 2,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(pt.X - 3, pt.Y - 3, 0, 0)
            };
            canvas.Children.Add(dot);
        }

        // ── Max value label ────────────────────────────────────
        var maxPt = points.OrderByDescending(p => p.Y).First();
        var label = new TextBlock
        {
            Text = $"${maxRevenue:N0}",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 124, 58, 237)),
            FontWeight = WpfFontWeights.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(maxPt.X + 4, maxPt.Y - 16, 0, 0)
        };
        canvas.Children.Add(label);
    }
}
