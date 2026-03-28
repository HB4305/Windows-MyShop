using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

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
