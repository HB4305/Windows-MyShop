using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace MyShop.Converters;

public class PercentageFormatter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal d)
        {
            return FormatPercentage(d);
        }

        if (value is double db)
        {
            return FormatPercentage((decimal)db);
        }

        if (value is float f)
        {
            return FormatPercentage((decimal)f);
        }

        if (value is string text)
        {
            var normalized = text.Trim().TrimEnd('%');
            if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.CurrentCulture, out var currentCultureValue) ||
                decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out currentCultureValue))
            {
                return FormatPercentage(currentCultureValue);
            }
        }

        if (value is IFormattable formattable)
        {
            return $"{formattable.ToString("0.00", CultureInfo.InvariantCulture)}%";
        }

        return value?.ToString() ?? string.Empty;
    }

    private static string FormatPercentage(decimal value)
    {
        return $"{value.ToString("0.00", CultureInfo.InvariantCulture)}%";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
