using System.Globalization;

namespace ppm_fe.Converters
{
    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                return doubleValue.ToString("F2", CultureInfo.InvariantCulture);
            }
            return "0.00";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (double.TryParse(value as string, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }
            return 0.0;
        }
    }
}
