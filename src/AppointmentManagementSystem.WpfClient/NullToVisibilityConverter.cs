using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AppointmentManagementSystem.WpfClient
{
    /// <summary>
    /// Converts null object to Visibility.Visible, non-null to Visibility.Collapsed.
    /// Used to show controls only when creating (SelectedAppointment is null).
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
