using System.Globalization;

namespace ppm_fe.Converters
{
    public class BoolToLoadingTextConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isLoading = value as bool? ?? false;
            string[] texts = parameter?.ToString()?.Split('|') ?? Array.Empty<string>();
            return isLoading ? texts[1] : texts[0];
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
