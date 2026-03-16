using System;
using System.Globalization;
using System.Windows.Data;

namespace AppointmentManagementSystem.WpfClient
{
    public sealed class SearchMatchConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                return false;
            }

            var text = values[0] as string ?? string.Empty;
            var query = values[1] as string ?? string.Empty;
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(query))
            {
                return false;
            }

            return text.IndexOf(query.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
