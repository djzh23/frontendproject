using System.Globalization;

namespace ppm_fe.Converters
{
    public class SelectedWorkConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] == null || values[1] == null)
                return Colors.Black;

            return values[0].Equals(values[1]) ? Colors.CornflowerBlue : Colors.Black;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
