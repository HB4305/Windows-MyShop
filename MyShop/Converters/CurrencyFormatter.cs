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
        if (value is decimal d)
        {
            return FormatDollar(d);
        }

        if (value is double db)
        {
            return FormatDollar((decimal)db);
        }

        if (value is float f)
        {
            return FormatDollar((decimal)f);
        }

        if (value is string text)
        {
            var normalized = text.Trim().Replace("$", string.Empty, StringComparison.Ordinal);
            if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.CurrentCulture, out var parsed) ||
                decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out parsed))
            {
                return FormatDollar(parsed);
            }
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
