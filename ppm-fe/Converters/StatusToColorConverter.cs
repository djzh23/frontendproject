using System.Globalization;

namespace ppm_fe.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                if(status == "complete")
                {
                    if (Application.Current == null)
                        return Colors.Green;

                    return Application.Current.Resources["SuccessColor"] ?? Colors.Green;
                }
                if (status == "inprogress")
                {
                    if (Application.Current == null)
                        return Colors.Orange;

                    return Application.Current.Resources["InProgressColor"] ?? Colors.Orange;
                }
            }

            if (Application.Current == null)
                return Colors.Pink;

            return Application.Current.Resources["PendingColor"] ?? Colors.Pink;
        }
        
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
