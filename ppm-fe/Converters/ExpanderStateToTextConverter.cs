using System.Globalization;

namespace ppm_fe.Converters
{
    public class ExpanderStateToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isExpanded = (bool)value;

            // Check if the parameter is "text" or "background"
            if (parameter.ToString() == "text")
            {
                return isExpanded ? "Minimieren \u25B2" : "Öffnen \u25BC";
            }
            else if (parameter.ToString() == "background")
            {
                return isExpanded ? Colors.LightGreen : Colors.LightPink;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
