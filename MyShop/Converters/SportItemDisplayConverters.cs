using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using MyShop.Models;
using Windows.UI;

namespace MyShop.Converters;

public sealed class SportItemStockPercentConverter : IValueConverter
{
    private static int ResolveStock(SportItem item) => item.EffectiveStockQuantity;

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not SportItem item)
            return 0.0;
        var stock = ResolveStock(item);
        var threshold = item.LowStockThreshold ?? 10;
        var denom = Math.Max(threshold * 8, 100);
        return Math.Min(100.0, stock * 100.0 / denom);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public sealed class SportItemStockBarForegroundConverter : IValueConverter
{
    private static int ResolveStock(SportItem item) => item.EffectiveStockQuantity;

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not SportItem item)
            return new SolidColorBrush(Color.FromArgb(255, 209, 213, 219));
        var stock = ResolveStock(item);
        var threshold = item.LowStockThreshold ?? 10;
        if (stock <= 0)
            return new SolidColorBrush(Color.FromArgb(255, 209, 213, 219));
        if (stock <= threshold)
            return new SolidColorBrush(Color.FromArgb(255, 245, 158, 11));
        return new SolidColorBrush(Color.FromArgb(255, 16, 185, 129));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public sealed class SportItemStatusTextConverter : IValueConverter
{
    private static int ResolveStock(SportItem item) => item.EffectiveStockQuantity;

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not SportItem item)
            return string.Empty;
        var stock = ResolveStock(item);
        var threshold = item.LowStockThreshold ?? 10;
        if (stock <= 0)
            return "Out of Stock";
        if (stock <= threshold)
            return "Low Stock";
        return "In Stock";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public sealed class SportItemStatusBadgeBackgroundConverter : IValueConverter
{
    private static int ResolveStock(SportItem item) => item.EffectiveStockQuantity;

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not SportItem item)
            return new SolidColorBrush(Color.FromArgb(255, 243, 244, 246));
        var stock = ResolveStock(item);
        var threshold = item.LowStockThreshold ?? 10;
        if (stock <= 0)
            return new SolidColorBrush(Color.FromArgb(255, 254, 226, 226));
        if (stock <= threshold)
            return new SolidColorBrush(Color.FromArgb(255, 254, 243, 199));
        return new SolidColorBrush(Color.FromArgb(255, 209, 250, 229));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public sealed class SportItemStatusBadgeForegroundConverter : IValueConverter
{
    private static int ResolveStock(SportItem item) => item.EffectiveStockQuantity;

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not SportItem item)
            return new SolidColorBrush(Color.FromArgb(255, 75, 85, 99));
        var stock = ResolveStock(item);
        var threshold = item.LowStockThreshold ?? 10;
        if (stock <= 0)
            return new SolidColorBrush(Color.FromArgb(255, 185, 28, 28));
        if (stock <= threshold)
            return new SolidColorBrush(Color.FromArgb(255, 180, 83, 9));
        return new SolidColorBrush(Color.FromArgb(255, 4, 120, 87));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
