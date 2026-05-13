using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace MyShop.Converters;

/// <summary>
/// Displays the value from DB correctly with a $ symbol (no conversion).
/// </summary>
public class CurrencyFormatter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        decimal? d = value switch
        {
            decimal val => val,
            double val => (decimal)val,
            float val => (decimal)val,
            int val => (decimal)val,
            string text => decimal.TryParse(text.Trim().Replace("$", string.Empty), NumberStyles.Number, CultureInfo.InvariantCulture, out var p) ? p : (decimal?)null,
            _ => null
        };

        if (parameter?.ToString() == "color")
        {
            if (d == null) return "#000000";
            return d < 0 ? "#DC2626" : "#16A34A"; // Red for negative, Green for positive/zero
        }

        if (d.HasValue)
        {
            return FormatDollar(d.Value);
        }

        return value?.ToString() ?? string.Empty;
    }

    private static string FormatDollar(decimal value)
    {
        var sign = value < 0 ? "-" : string.Empty;
        return $"{sign}${Math.Abs(value).ToString("N2", CultureInfo.InvariantCulture)}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
