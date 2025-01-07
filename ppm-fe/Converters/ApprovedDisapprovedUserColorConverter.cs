using System.Globalization;

namespace ppm_fe.Converters
{
    public class ApprovedDisapprovedUserColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool approved)
            {
                if (approved)
                {
                    if (Application.Current == null)
                        return Colors.Green;

                    return Application.Current.Resources["ApproveColor"] ?? Colors.Green;
                }
            }

            if (Application.Current == null)
                return Colors.Red;

            return Application.Current.Resources["DissaproveColor"];
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
