using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace MyShop.Converters;

/// <summary>Chuyển null → Visibility.Collapsed, non-null → Visibility.Visible</summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
        => value != null && !string.IsNullOrWhiteSpace(value.ToString())
            ? Visibility.Visible
            : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>Chuyển null → false, non-null → true</summary>
public class NullToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
        => value != null;

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>Chuyển true → Visibility.Visible, false → Visibility.Collapsed</summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>Đảo ngược bool: true → false, false → true</summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
        => value is bool b ? !b : true;

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => value is bool b ? !b : true;
}

/// <summary>Chuyển string rỗng → Collapsed, có nội dung → Visible</summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
        => !string.IsNullOrWhiteSpace(value?.ToString())
            ? Visibility.Visible
            : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Chuyển bool → Foreground color. Dùng cho ConfigPage message.
/// true  → xanh lá (0, 200, 0)
/// false → đỏ   (255, 80, 80)
/// </summary>
public class BoolToForegroundConverter : IValueConverter
{
    private static readonly SolidColorBrush GreenBrush = new(
        Microsoft.UI.ColorHelper.FromArgb(255, 34, 197, 94));   // #22C55E
    private static readonly SolidColorBrush RedBrush = new(
        Microsoft.UI.ColorHelper.FromArgb(255, 239, 68, 68));    // #EF4444
    private static readonly SolidColorBrush BlackBrush = new(
        Microsoft.UI.ColorHelper.FromArgb(255, 0, 0, 0));

    public object? Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is not bool isSuccess)
            return BlackBrush;

        return isSuccess ? GreenBrush : RedBrush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}
