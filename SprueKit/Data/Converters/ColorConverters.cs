using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

using Microsoft.Xna.Framework;

namespace SprueKit.Data
{

    public class XNAColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Microsoft.Xna.Framework.Color c = (Microsoft.Xna.Framework.Color)value;
            return System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Windows.Media.Color c = (System.Windows.Media.Color)value;
            return new Microsoft.Xna.Framework.Color(c.R, c.G, c.B, c.A);
        }
    }

    public class ColorToBrushConverter : IValueConverter
    {
    
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Vector4)
            {
                return new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
            }
            else
            {
                Microsoft.Xna.Framework.Color c = (Microsoft.Xna.Framework.Color)value;
                return new SolidColorBrush(System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B));
            }
        }
    
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    //
    //public class ColorToInvertedBrushConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        if (!(value is Vector4))
    //            return new SolidColorBrush(Color.FromRgb(255, 255, 255));
    //        Vector4 c = (Vector4)value;
    //        int sum = (int)(c.X * 255 + c.Y * 255 + c.Z * 255);
    //        Color col = Color.FromRgb((byte)(255 - c.X * 255), (byte)(255 - c.Y * 255), (byte)(255 - c.Z * 255));
    //        int nsum = col.R + col.G + col.B;
    //        if (Math.Max(nsum, sum) - Math.Min(nsum, sum) < 128)
    //        {
    //            if (nsum > 128 * 3) //new color is brighter
    //                return new SolidColorBrush(Colors.Black);
    //            return new SolidColorBrush(Colors.White);
    //        }
    //        return new SolidColorBrush((Color)col);
    //    }
    //
    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        return null;
    //    }
    //}
}
