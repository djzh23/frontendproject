using ppm_fe.Models;
using System.Globalization;

namespace ppm_fe.Converters
{
    class WorkUpdateButtonVisibleConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            Work? work = value as Work ?? null;
            if (work == null)
            {
                return false;
            }
            return work.Status == "standing" || work.Status == "inprogress";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();

        }
    }
}
