using System.Globalization;

namespace ppm_fe.Converters
{
    public class DateOnlyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string dateString)
            {
                if (DateTime.TryParse(dateString, out DateTime date))
                {
                    return $"{date:yyyy-MM-dd}";
                }
            }
            return "Date: Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
