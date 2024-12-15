using System.Globalization;

namespace ppm_fe.Converters
{
    public class DateTimeStringToTimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string dateTimeString)
            {
                if (DateTimeOffset.TryParse(dateTimeString, out DateTimeOffset dateTimeOffset))
                {
                    // Adjust by subtracting the offset to get the intended local time
                    DateTime adjustedTime = dateTimeOffset.UtcDateTime.Add(dateTimeOffset.Offset);
                    return adjustedTime.TimeOfDay;
                }
            }
            return TimeSpan.Zero;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                if (parameter is string originalDateTimeString &&
                    DateTimeOffset.TryParse(originalDateTimeString, out DateTimeOffset originalDateTimeOffset))
                {
                    // Create a new DateTimeOffset with the adjusted time
                    DateTime newDateTime = originalDateTimeOffset.UtcDateTime.Add(timeSpan - originalDateTimeOffset.TimeOfDay);
                    return newDateTime.ToString("o");
                }
                // If no original date is provided, use current date with local offset
                return DateTimeOffset.Now.Date.Add(timeSpan).ToString("o");
            }
            return DateTimeOffset.Now.ToString("o");
        }
    }
}
