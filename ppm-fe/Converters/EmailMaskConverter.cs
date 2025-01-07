using System.Globalization;

namespace ppm_fe.Converters
{
    public class EmailMaskConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string email)
            {
                if (string.IsNullOrEmpty(email))
                    return string.Empty;

                if (email.Length <= 4)
                    return email;

                // Masking the email ín the Flyout Menu Header 
                return email.Substring(0, 4) + new string('*', email.Length - 4);
            }
            return string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
