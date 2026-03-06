using System;
using System.Globalization;
using System.Windows.Data;

namespace AppointmentManagementSystem.WpfClient
{
    /// <summary>
    /// Converts a SelectedAppointment object to a header string.
    /// If null (creating new), returns "Create Appointment"
    /// If not null (editing), returns "Edit Appointment"
    /// </summary>
    public class SelectedAppointmentToHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "Create Appointment";
            }

            return "Edit Appointment";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
