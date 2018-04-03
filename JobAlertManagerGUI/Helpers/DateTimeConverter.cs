using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Resources;
using System.Reflection;

namespace JobAlertManagerGUI.Helpers
{
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                if (value.GetType() == typeof(DateTime))
                    return ((DateTime)value).ToString();
                else if (value.GetType() == typeof(DateTime?))
                {
                    DateTime? dt = value as DateTime?;
                    if (dt.HasValue)
                        return dt.Value.ToString();
                    else
                        return "";
                }
                else if (value.GetType() == typeof(DateTimeOffset))
                {
                    return ((DateTimeOffset)value).LocalDateTime.ToString();
                }
                else
                    return "???";
            }
            else
                return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
