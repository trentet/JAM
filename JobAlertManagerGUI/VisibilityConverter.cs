using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JobAlertManagerGUI
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var b = (bool) value;
                return b ? Visibility.Visible : Visibility.Collapsed;
            }
            catch
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var v = (Visibility) value;
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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var b = (bool) value;
                return !b ? Visibility.Visible : Visibility.Collapsed;
            }
            catch
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var v = (Visibility) value;
                return v == Visibility.Visible ? false : true;
            }
            catch
            {
                return false;
            }
        }
    }
}