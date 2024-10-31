using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using System.Reflection;

namespace SprueKit.Data.Converters
{
    public class InvertBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TypeOfVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return System.Windows.Visibility.Collapsed;
            return ((Type)parameter).IsAssignableFrom(value.GetType()) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ShapeVisibilityConverter : IValueConverter
    {
        PropertyInfo property;
        int index;

        public ShapeVisibilityConverter(PropertyInfo pi, int index)
        {
            property = pi;
            this.index = index;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SprueKit.Data.ShapeFunctionType func = (SprueKit.Data.ShapeFunctionType)value;
            switch (func)
            {
            case ShapeFunctionType.Sphere:
                return index == 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            case ShapeFunctionType.Cone:
            case ShapeFunctionType.Capsule:
            case ShapeFunctionType.Torus:
            case ShapeFunctionType.Cylinder:
                return index < 2 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            case ShapeFunctionType.Box:
            case ShapeFunctionType.Ellipsoid:
                return index < 3 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            case ShapeFunctionType.Plane:
            case ShapeFunctionType.RoundedBox:
            case ShapeFunctionType.SuperShape:
                return System.Windows.Visibility.Visible;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
