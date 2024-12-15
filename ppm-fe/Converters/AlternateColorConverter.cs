using ppm_fe.Models;
using System.Globalization;

namespace ppm_fe.Converters
{
    public class AlternateColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var listView = parameter as ListView;
            if (listView == null || value == null)
                return Colors.Transparent;

            var item = value as BillingDetail;
            var items = listView.ItemsSource as IList<BillingDetail>;
            if (items == null)
                return Colors.Transparent;

            int index = items.IndexOf(item);
            return index % 2 == 0 ? Color.FromHex("#F8F8F8") : Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
