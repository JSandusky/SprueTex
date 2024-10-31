using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace SprueKit.Controls.Converters
{
    public class DefaultPropertyConverter
    {
    }

    public class DefaultPropertyColorConverter : IValueConverter
    {
        PropertyInfo pi_;
        Brush normalBrush_;
        Brush editedBrush_;

        public DefaultPropertyColorConverter(PropertyInfo pi)
        {
            pi_ = pi;
            //#333333
            normalBrush_ = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xc1, 0xc1, 0xc1));// LightSlateGray);
            editedBrush_ = new SolidColorBrush(System.Windows.Media.Colors.LimeGreen);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Data.DefaultPool.IsPropertyDefault(parameter, pi_))
                return normalBrush_;
            return editedBrush_;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
