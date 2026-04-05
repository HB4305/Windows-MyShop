using System.Collections.Generic;
using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace MyShop.Converters;

/// <summary>
/// Format OrderId → "#ATH-XXXXX" với số 5 chữ số, prefix "ATH-".
/// </summary>
public class OrderAthIdConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is int id)
            return $"#ATH-{id.ToString("D5")}";
        return "#ATH------";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Lấy chữ cái đầu của tên (Avatar initials).
/// </summary>
public class InitialsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        var name = value as string ?? "";
        var parts = name.Trim().Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "?";
        if (parts.Length == 1) return parts[0][0].ToString().ToUpper();
        return (parts[0][0].ToString() + parts[^1][0].ToString()).ToUpper();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Format DateTimeOffset? → "MMM dd, h:mm tt" (Oct 24, 2:45 PM)
/// </summary>
public class OrderDateConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is DateTimeOffset dto)
            return dto.ToString("MMM dd, h:mm tt", CultureInfo.InvariantCulture);
        return "-";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Chuyển Order.PaymentStatus → SolidColorBrush cho badge background.
/// Hỗ trợ thêm trạng thái "Created".
/// </summary>
public class PayBadgeBgConverter : IValueConverter
{
    private static readonly Dictionary<string, string> PayBg = new(System.StringComparer.OrdinalIgnoreCase)
    {
        ["Paid"]    = "#D1FAE5",
        ["Unpaid"]  = "#FEE2E2",
        ["Created"] = "#DBEAFE",
    };

    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        var status = value as string ?? "";
        var hex = PayBg.TryGetValue(status, out var h) ? h : "#F3F4F6";
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
/// Chuyển Order.PaymentStatus → SolidColorBrush cho text foreground.
/// Hỗ trợ thêm trạng thái "Created".
/// </summary>
public class PayBadgeFgConverter : IValueConverter
{
    private static readonly Dictionary<string, string> PayFg = new(System.StringComparer.OrdinalIgnoreCase)
    {
        ["Paid"]    = "#065F46",
        ["Unpaid"]  = "#991B1B",
        ["Created"] = "#1E40AF",
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

