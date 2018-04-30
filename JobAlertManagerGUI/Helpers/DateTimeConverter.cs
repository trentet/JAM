using System;
using System.Globalization;
using System.Windows.Data;

namespace JobAlertManagerGUI.Helpers
{
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                if (value.GetType() == typeof(DateTime))
                {
                    return ((DateTime) value).ToString();
                }
                else if (value.GetType() == typeof(DateTime?))
                {
                    var dt = value as DateTime?;
                    if (dt.HasValue)
                        return dt.Value.ToString();
                    return "";
                }
                else if (value.GetType() == typeof(DateTimeOffset))
                {
                    return ((DateTimeOffset) value).LocalDateTime.ToString();
                }
                else
                {
                    return "???";
                }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}