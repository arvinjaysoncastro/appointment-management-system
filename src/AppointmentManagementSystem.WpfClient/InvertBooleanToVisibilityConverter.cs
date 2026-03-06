using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AppointmentManagementSystem.WpfClient
{
    /// <summary>
    /// Converts boolean to visibility (inverted) - True becomes Hidden, False becomes Visible.
    /// </summary>
    public class InvertBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Hidden : Visibility.Visible;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility != Visibility.Visible;
            }

            return false;
        }
    }
}
