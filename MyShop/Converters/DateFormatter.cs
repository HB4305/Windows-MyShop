using System;
using Microsoft.UI.Xaml.Data;

namespace MyShop.Converters;

public class DateFormatter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var format = parameter as string ?? "dd/MM/yyyy";

        if (value is DateTime dateTime)
        {
            return dateTime.ToString(format);
        }

        if (value is DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.ToString(format);
        }

        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
