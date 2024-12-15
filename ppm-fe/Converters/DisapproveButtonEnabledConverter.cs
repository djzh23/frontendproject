using System.Globalization;

namespace ppm_fe.Converters
{
    public class DisapproveButtonEnabledConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            ArgumentNullException.ThrowIfNull(value);
            return (int)value != 5; // Enable "Disapprove" button if RoleId is not 5 / Unknown
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
