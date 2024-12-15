using System.Globalization;

namespace ppm_fe.Converters
{
    public class DateFormatConverter : IValueConverter
    {
        private const string DatabaseDateFormat = "yyyy-M-d";
        private const string DisplayDateFormat = "MM/dd/yyyy";

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(parameter);

            if (value is DateTime dateTime)
            {
                return dateTime.ToString(DisplayDateFormat);
            }

            if (value is string dateString)
            {
                if (DateTime.TryParseExact(dateString, DatabaseDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDateTime))
                {
                    return parsedDateTime.ToString(DisplayDateFormat);
                }
            }

            return DateTime.Now.ToString(DisplayDateFormat);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(parameter);

            if (value is string dateString)
            {
                // Supports multiple date formats
                string[] formats = [DisplayDateFormat, DatabaseDateFormat, "yyyy-MM-dd", "MM/dd/yyyy"];
                if (DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDateTime))
                {
                    return parsedDateTime.ToString(DatabaseDateFormat);
                }
            }

            if (value is DateTime dateTime)
            {
                return dateTime.ToString(DatabaseDateFormat);
            }

            return DateTime.Now.ToString(DatabaseDateFormat);
        }
    }
}
