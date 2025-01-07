using System.Globalization;

namespace ppm_fe.Converters
{
    internal class ApproveDisapproveButtonVisbilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            ArgumentNullException.ThrowIfNull(value);
            return !(bool)value;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
