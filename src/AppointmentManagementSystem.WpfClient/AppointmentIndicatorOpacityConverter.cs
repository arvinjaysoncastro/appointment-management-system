using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using AppointmentManagementSystem.WpfClient.Models;

namespace AppointmentManagementSystem.WpfClient
{
    public sealed class AppointmentIndicatorOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var appointment = value as AppointmentModel;
            if (appointment == null)
            {
                return new List<double> { 0d };
            }

            var totalMinutes = Math.Max(0d, (appointment.End - appointment.Start).TotalMinutes);
            var circleCount = Math.Max(1, (int)Math.Ceiling(totalMinutes / 60d));
            var opacities = new List<double>(circleCount);

            for (var i = 0; i < circleCount; i++)
            {
                var remainingForCircle = Math.Max(0d, totalMinutes - (i * 60d));
                var circleMinutes = Math.Min(60d, remainingForCircle);
                opacities.Add(circleMinutes / 60d);
            }

            return opacities;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
