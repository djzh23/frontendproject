using System.Globalization;

namespace ppm_fe.Converters
{
    public class ExpanderStateToTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isExpanded = value as bool? ?? false;

            if (parameter?.ToString() == "text")
            {
                return isExpanded ? "Einsätze ausblenden \u25B2" : "Einsätze öffnen \u25BC";
            }
            else if (parameter?.ToString() == "background")
            {
                return isExpanded ? Colors.LightGreen : Colors.LightPink;
            }

            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
