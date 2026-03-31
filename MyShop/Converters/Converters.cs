using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

/// <summary>Chuyển hex color string (#RRGGBB) thành SolidColorBrush</summary>
public class HexToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        var hex = value as string;
        if (string.IsNullOrWhiteSpace(hex)) return new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));

        hex = hex.TrimStart('#');
        if (hex.Length == 6)
        {
            byte r = System.Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = System.Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = System.Convert.ToByte(hex.Substring(4, 2), 16);
            return new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Windows.UI.Color.FromArgb(255, r, g, b));
        }
        return new Microsoft.UI.Xaml.Media.SolidColorBrush(
            Windows.UI.Color.FromArgb(0, 0, 0, 0));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>Lấy ảnh đầu tiên trong danh sách ảnh (array text) để hiển thị thumbnail</summary>
public class FirstImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is IEnumerable<string> list && list.Any())
        {
            var url = list.First();
            if (!string.IsNullOrWhiteSpace(url))
            {
                try
                {
                    return new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new System.Uri(url));
                }
                catch { }
            }
        }
        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>Chuyển string URL thành BitmapImage để dùng cho Image.Source trong DataTemplate</summary>
public class StringToImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is string url && !string.IsNullOrWhiteSpace(url))
        {
            try
            {
                return new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new System.Uri(url));
            }
            catch { }
        }
        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>Chuyển string rỗng -> false, có nội dung -> true</summary>
public class StringToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
        => !string.IsNullOrWhiteSpace(value?.ToString());

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}
