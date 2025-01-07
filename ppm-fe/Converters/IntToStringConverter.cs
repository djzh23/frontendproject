using System.Globalization;

namespace ppm_fe.Converters
{
    public class IntToStringConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue.ToString();
            }
            return string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value as string))
            {
                return 0; // Return 0 for empty or whitespace input
            }

            if (int.TryParse(value as string, out int result))
            {
                return result >= 0 ? result : 0; // Ensures non-negative numbers
            }

            return 0; // Default to 0 for invalid input
        }
    }
}
