using System.Globalization;

namespace ppm_fe.Converters
{
    public class DisapproveButtonEnabledConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            ArgumentNullException.ThrowIfNull(value);
            return (int)value != 5;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
