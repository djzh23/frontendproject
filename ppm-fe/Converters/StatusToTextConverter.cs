using System.Globalization;

namespace ppm_fe.Converters
{
    public class StatusToTextConverter : IValueConverter
    {
        // Here we can modify the text of work status
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                if (status == "complete")
                {
                    return "complete";
                }
                if (status == "inprogress")
                {
                    return "in progress";
                }
            }

            return "standing";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
