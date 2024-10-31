using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Microsoft.Xna.Framework;
using System.Windows.Media;

namespace SprueKit.Controls.Converters
{
    public class XNAColorToBrushConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is Microsoft.Xna.Framework.Color))
                return new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
            return new SolidColorBrush(((Microsoft.Xna.Framework.Color)value).ToMediaColor());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

    }

    public class XNAColorToGradientConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is Microsoft.Xna.Framework.Color))
                return new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
            return new LinearGradientBrush(Colors.Black, ((Microsoft.Xna.Framework.Color)value).ToMediaColor(), 90) { MappingMode = BrushMappingMode.RelativeToBoundingBox };
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

    }

    public class XNAColorToInvertedBrushConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is Microsoft.Xna.Framework.Color))
                return new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
            Microsoft.Xna.Framework.Color c = (Microsoft.Xna.Framework.Color)value;
            int sum = c.R + c.G + c.B;
            System.Windows.Media.Color col = System.Windows.Media.Color.FromRgb((byte)(255 - c.R), (byte)(255 - c.G), (byte)(255 - c.B));
            int nsum = col.R + col.G + col.B;
            if (Math.Max(nsum, sum) - Math.Min(nsum, sum) < 128)
            {
                if (nsum > 128 * 3) //new color is brighter
                    return new SolidColorBrush(Colors.Black);
                return new SolidColorBrush(Colors.White);
            }
            return new SolidColorBrush(col);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

    }
}
