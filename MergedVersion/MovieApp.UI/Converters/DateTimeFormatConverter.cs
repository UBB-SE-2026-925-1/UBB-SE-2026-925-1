using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace MovieApp.UI.Converters;

public sealed class DateTimeFormatConverter : IValueConverter
{
    public string Format { get; set; } = "g";

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime dt)
        {
            return dt.ToString(this.Format, CultureInfo.CurrentCulture);
        }

        if (value is DateTimeOffset dto)
        {
            return dto.ToString(this.Format, CultureInfo.CurrentCulture);
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
