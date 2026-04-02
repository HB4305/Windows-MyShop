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

/// <summary>Chuyển true → Collapsed, false → Visible (tiêu đề “tạo mới” khi chưa có Id).</summary>
public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
        => value is true ? Visibility.Collapsed : Visibility.Visible;

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

/// <summary>Chuỗi rỗng → Visible (placeholder); có URL → Collapsed.</summary>
public class InverseStringToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
        => string.IsNullOrWhiteSpace(value?.ToString())
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

/// <summary>
/// Chuyển Order.Status → SolidColorBrush cho badge background.
/// </summary>
public class OrderStatusToBrushConverter : IValueConverter
{
    private static readonly Dictionary<string, (string bg, string fg)> StatusStyles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Pending"]    = ("#FEF3C7", "#92400E"),  // amber
        ["Processing"] = ("#DBEAFE", "#1E40AF"),  // blue
        ["Shipped"]    = ("#E0E7FF", "#3730A3"),  // indigo
        ["Delivered"]  = ("#D1FAE5", "#065F46"),  // green
        ["Cancelled"]  = ("#FEE2E2", "#991B1B"),  // red
    };

    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        var status = value as string ?? "";
        if (!StatusStyles.TryGetValue(status, out var style))
            style = ("#F3F4F6", "#6B7280"); // gray default

        var bg = ParseHex(style.bg);
        return new SolidColorBrush(bg);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();

    private static Windows.UI.Color ParseHex(string hex)
    {
        hex = hex.TrimStart('#');
        return Windows.UI.Color.FromArgb(255,
            System.Convert.ToByte(hex.Substring(0, 2), 16),
            System.Convert.ToByte(hex.Substring(2, 2), 16),
            System.Convert.ToByte(hex.Substring(4, 2), 16));
    }
}

/// <summary>
/// Chuyển Order.PaymentStatus → SolidColorBrush cho text foreground.
/// </summary>
public class PaymentStatusToForegroundConverter : IValueConverter
{
    private static readonly Dictionary<string, string> PayFg = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Paid"]   = "#065F46",
        ["Unpaid"] = "#991B1B",
    };

    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        var status = value as string ?? "";
        var hex = PayFg.TryGetValue(status, out var h) ? h : "#6B7280";
        hex = hex.TrimStart('#');
        return new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255,
            System.Convert.ToByte(hex.Substring(0, 2), 16),
            System.Convert.ToByte(hex.Substring(2, 2), 16),
            System.Convert.ToByte(hex.Substring(4, 2), 16)));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Chuyển Order.PaymentStatus → SolidColorBrush.
/// </summary>
public class PaymentStatusToBrushConverter : IValueConverter
{
    private static readonly Dictionary<string, (string bg, string fg)> PayStyles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Paid"]   = ("#D1FAE5", "#065F46"),   // green
        ["Unpaid"] = ("#FEE2E2", "#991B1B"),   // red
    };

    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        var status = value as string ?? "";
        if (!PayStyles.TryGetValue(status, out var style))
            style = ("#F3F4F6", "#6B7280");

        var bg = ParseHex(style.bg);
        return new SolidColorBrush(bg);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();

    private static Windows.UI.Color ParseHex(string hex)
    {
        hex = hex.TrimStart('#');
        return Windows.UI.Color.FromArgb(255,
            System.Convert.ToByte(hex.Substring(0, 2), 16),
            System.Convert.ToByte(hex.Substring(2, 2), 16),
            System.Convert.ToByte(hex.Substring(4, 2), 16));
    }
}

/// <summary>
/// Chuyển Order.Status → SolidColorBrush cho text foreground.
/// </summary>
public class OrderStatusToForegroundConverter : IValueConverter
{
    private static readonly Dictionary<string, string> StatusFg = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Pending"]    = "#92400E",
        ["Processing"] = "#1E40AF",
        ["Shipped"]    = "#3730A3",
        ["Delivered"]  = "#065F46",
        ["Cancelled"]  = "#991B1B",
    };

    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        var status = value as string ?? "";
        var hex = StatusFg.TryGetValue(status, out var h) ? h : "#6B7280";
        hex = hex.TrimStart('#');
        return new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255,
            System.Convert.ToByte(hex.Substring(0, 2), 16),
            System.Convert.ToByte(hex.Substring(2, 2), 16),
            System.Convert.ToByte(hex.Substring(4, 2), 16)));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Format decimal → currency string "$ {N2}"
/// </summary>
public class CurrencyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is decimal d)
            return $"${d:N2}";
        return "$0.00";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Format DateTimeOffset? → "dd/MM/yyyy HH:mm"
/// </summary>
public class DateTimeOffsetConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is DateTimeOffset dto)
            return dto.ToString("dd/MM/yyyy HH:mm");
        return "-";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Tính line total: Quantity × UnitPrice
/// </summary>
public class LineTotalConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is Models.OrderDetail od)
        {
            var total = od.Quantity * od.UnitPrice;
            return $"${total:N2}";
        }
        return "$0.00";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Format OrderId → "#123" string (dùng trong x:Bind Run without mixed Run)
/// </summary>
public class OrderIdConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is int id)
            return $"#{id}";
        return "#-";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Chuyển int Count → Visibility. 0 → Visible (empty state), >0 → Collapsed.
/// </summary>
public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is int count)
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Chuyển int Count → Visibility. >0 → Visible, 0 → Collapsed.
/// </summary>
public class CountToVisibilityInverseConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is int count)
            return count > 0 ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}
