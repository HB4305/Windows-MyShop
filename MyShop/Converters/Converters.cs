using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace MyShop.Converters;

/// <summary>Converts null → Visibility.Collapsed, non-null → Visibility.Visible</summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
        => value != null && !string.IsNullOrWhiteSpace(value.ToString())
            ? Visibility.Visible
            : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>Converts null → false, non-null → true</summary>
public class NullToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
        => value != null;

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>Converts true → Visibility.Visible, false → Visibility.Collapsed</summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>Converts true → Collapsed, false → Visible (used for "Create New" title when no Id exists).</summary>
public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
        => value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>Inverts bool: true → false, false → true</summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
        => value is bool b ? !b : true;

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => value is bool b ? !b : true;
}

/// <summary>Converts empty string → Collapsed, has content → Visible</summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
        => !string.IsNullOrWhiteSpace(value?.ToString())
            ? Visibility.Visible
            : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>Empty string → Visible (placeholder); has URL → Collapsed.</summary>
public class InverseStringToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
        => string.IsNullOrWhiteSpace(value?.ToString())
            ? Visibility.Visible
            : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// Converts bool → Foreground color. Used for ConfigPage messages.
/// true  → green (#22C55E)
/// false → red   (#EF4444)
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

/// <summary>Converts hex color string (#RRGGBB) to SolidColorBrush</summary>
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

/// <summary>Gets the first image from an image list (array text) for thumbnail display</summary>
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
                    var bitmap = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage();
                    bitmap.DecodePixelWidth = 88; // 44px * 2 for high DPI
                    bitmap.UriSource = new System.Uri(url);
                    return bitmap;
                }
                catch { }
            }
        }
        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>Converts string URL to BitmapImage for Image.Source in DataTemplate</summary>
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

/// <summary>Converts empty string -> false, has content -> true</summary>
public class StringToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
        => !string.IsNullOrWhiteSpace(value?.ToString());

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// Converts Order.Status → SolidColorBrush for the badge background.
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

/// Converts Order.PaymentStatus → SolidColorBrush for the text foreground.
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

/// Converts Order.PaymentStatus → SolidColorBrush.
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

/// Converts Order.Status → SolidColorBrush for the text foreground.
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

/// Formats DateTimeOffset? → "dd/MM/yyyy HH:mm"
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

/// Calculates line total: Quantity × UnitPrice
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

/// Formats OrderId → "#123" string (used in x:Bind Run)
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

/// Converts int Count → Visibility. 0 → Visible (empty state), >0 → Collapsed.
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

/// Calculates the end index of the current page: CurrentPage * PageSize
public class CurrentPageEndConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (parameter is string pageSizeStr && int.TryParse(pageSizeStr, out var pageSize))
        {
            if (value is int currentPage)
                return Math.Min(currentPage * pageSize, 9999); // Will bind to actual TotalOrders
        }
        return 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// Gets the index of an item in the list to display "1, 2, 3..."
public class ItemIndexConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is int index)
            return (index + 1).ToString();
        return "0";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// Converts int Count → Visibility. >0 → Visible, 0 → Collapsed.
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
