using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace JobAlertManagerGUI
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var b = (bool)value;
                return b ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
            catch
            {
                return System.Windows.Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                Visibility v = (Visibility)value;
                return v == Visibility.Visible ? true : false;
            }
            catch
            {
                return true;
            }
        }
    }

    public class InVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var b = (bool)value;
                return !b ? Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
            catch
            {
                return System.Windows.Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                Visibility v = (System.Windows.Visibility)value;
                return v == Visibility.Visible ? false : true;
            }
            catch
            {
                return false;
            }
        }
    }
}
