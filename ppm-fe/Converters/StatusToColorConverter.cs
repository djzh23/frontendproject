using System.Globalization;

namespace ppm_fe.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                //return status.ToLower() == "complete" ? Colors.Green : Colors.Red;
                return status.ToLower() == "complete" ? Color.FromRgba(0, 170, 0, 0.8) : Color.FromRgba(135, 162, 86, 1.0);
            }
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
