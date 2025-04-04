﻿using System.Globalization;

namespace ppm_fe.Converters
{
    public class SelectedWorkConverter : IMultiValueConverter
    {
        public object Convert(object[]? values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (Application.Current == null)
                return Colors.Black; // default color

            if (values?.Length < 2 || values?[0] == null || values[1] == null)
                return Application.Current.Resources["UnselectedWorkTextColor"] ?? Colors.Black;

            return values[0].Equals(values[1])
            ? Application.Current.Resources["SelectedWorkTextColor"] ?? Colors.Black
            : Application.Current.Resources["UnselectedWorkTextColor"] ?? Colors.Gray;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
