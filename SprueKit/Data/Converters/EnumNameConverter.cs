using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SprueKit.Data.Converters
{
    public class EnumNameConverter : EnumConverter
    {
        public EnumNameConverter(Type type) : base(type)
        {

        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return base.ConvertTo(context, culture, value, destinationType).ToString().SplitCamelCase();
        }
    }

    public class ShapeTypeTooltipConverter : IValueConverter
    {
        int index;

        public ShapeTypeTooltipConverter(int index)
        {
            this.index = index;
        }

        string[,] tips =
        {
            { "Radius", "", "", "" }, // Sphere
            { "X", "Y", "Z", "" }, // Box,
            { "X", "Y", "Z", "Roundness" }, // RoundedBox,
            { "Radius", "Length", "", "" }, // Capsule,
            { "Radius", "Height", "", "" }, // Cylinder
            { "Radius", "Height", "", "" }, // Cone,
            { "X", "Y", "Z", "" }, // Ellipsoid,
            { "X", "Y", "Z", "Distance" }, // Plane,
            { "Inner Radius", "Outer Radius", "", "" }, // Torus,
            { "S", "T", "U", "V" }, // SuperShape,
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SprueKit.Data.ShapeFunctionType type = (SprueKit.Data.ShapeFunctionType)value;

            if (index == 4)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < 4; ++i)
                {
                    if (!String.IsNullOrEmpty(tips[(int)type, i]))
                    {
                        if (i > 0)
                            sb.Append(", ");
                        sb.Append(tips[(int)type, i]);
                    }
                    else
                        break;
                }
                return sb.ToString();
            }
            return tips[(int)type, index];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
