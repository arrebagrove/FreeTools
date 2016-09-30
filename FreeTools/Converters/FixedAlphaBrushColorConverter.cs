using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace FreeTools.Converters
{
    public class FixedAlphaBrushColorConverter : DependecyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Color)
            {
                var color = (Color)value;
                return Color.FromArgb(
                    System.Convert.ToByte(color.A * 0.75), 
                    color.R, 
                    color.G, 
                    color.B);
            }
            else if (value is SolidColorBrush)
            {
                var solidColorBrush = (SolidColorBrush)value;
                return new SolidColorBrush(
                    Color.FromArgb(
                        System.Convert.ToByte(solidColorBrush.Color.A * 0.75), 
                        solidColorBrush.Color.R, 
                        solidColorBrush.Color.G, 
                        solidColorBrush.Color.B));
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
