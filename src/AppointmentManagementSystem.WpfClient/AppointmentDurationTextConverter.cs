using System;
using System.Globalization;
using System.Windows.Data;
using AppointmentManagementSystem.WpfClient.Models;

namespace AppointmentManagementSystem.WpfClient
{
    public sealed class AppointmentDurationTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var appointment = value as AppointmentModel;
            if (appointment == null)
            {
                return "0m";
            }

            var totalMinutes = Math.Max(0, (int)Math.Round((appointment.End - appointment.Start).TotalMinutes, MidpointRounding.AwayFromZero));
            var hours = totalMinutes / 60;
            var minutes = totalMinutes % 60;

            if (hours > 0 && minutes > 0)
            {
                return hours + "h " + minutes + "m";
            }

            if (hours > 0)
            {
                return hours + "h";
            }

            return minutes + "m";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
