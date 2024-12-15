using System.Globalization;

namespace ppm_fe.Converters
{
    public class ApproveButtonEnabledConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            ArgumentNullException.ThrowIfNull(value);
            return (int)value == 5; // Enable "Approve" button if RoleId is 5 / Unknown
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
