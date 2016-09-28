using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace FreeTools.Converters
{
    public class StringLengthToOpacityConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
            {
                var str = (string)value;

                return !string.IsNullOrWhiteSpace(str) && str.Length > 0 ? 0.5 : 1.0;
            }

            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
