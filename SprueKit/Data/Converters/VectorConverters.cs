using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Reflection;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using System.Globalization;

namespace SprueKit.Data
{
    /// <summary>
    /// Vector2<->string conversion
    /// </summary>
    public class Vec2StrConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is Vector2)
            {
                return ((Vector2)value).ToString();
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = value.ToString();
            return DataExtensions.ParseVector2(str);
        }
    }

    /// <summary>
    /// CODE SIDE ONLY! Maps a vector element to binding.
    /// Requires a property and object for coherency
    /// </summary>
    public class Vec2ElemConverter : IValueConverter
    {
        int elemIndex = 0;
        PropertyInfo property;
        object srcObject;

        public Vec2ElemConverter(int elemIdx, object src, PropertyInfo prop)
        {
            Debug.Assert(elemIdx == 0 || elemIdx == 1, "Invalid index for Vec2ElemConverter");
            elemIndex = elemIdx;
            srcObject = src;
            property = prop;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is Vector2)
            {
                switch (elemIndex)
                {
                    case 0:
                        return ((Vector2)value).X;
                    case 1:
                        return ((Vector2)value).Y;
                }
            }
            else if (value != null && value is PluginLib.IntVector2)
            {
                switch (elemIndex)
                {
                    case 0:
                        return ((PluginLib.IntVector2)value).X;
                    case 1:
                        return ((PluginLib.IntVector2)value).Y;
                }
            }
            return 0.0f;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            float val = 0.0f;
            if (float.TryParse(value.ToString(), out val))
            {
                object v = property.GetValue(srcObject);
                if (v is Vector2)
                {
                    Vector2 curValue = (Vector2)v;
                    switch (elemIndex)
                    {
                        case 0:
                            curValue.X = val;
                            break;
                        case 1:
                            curValue.Y = val;
                            break;
                    }
                    return curValue;
                }
                else if (v is PluginLib.IntVector2)
                {
                    PluginLib.IntVector2 curValue = (PluginLib.IntVector2)v;
                    switch (elemIndex)
                    {
                        case 0:
                            curValue.X = (int)val;
                            break;
                        case 1:
                            curValue.Y = (int)val;
                            break;
                    }
                    return curValue;
                }
            }
            return new Vector2();
        }
    }

    /// <summary>
    /// Vector3<->string converter
    /// </summary>
    public class Vec3StrConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is Vector3)
            {
                return ((Vector3)value).ToString();
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = value.ToString();
            return DataExtensions.ParseVector3(str);
        }
    }

    /// <summary>
    /// USE FROM CODE ONLY! Maps a vector element to a binding.
    /// Requires a property and object for coherency
    /// </summary>
    public class Vec3ElemConverter : IValueConverter
    {
        int elemIndex = 0;
        PropertyInfo property;
        object srcObject;

        public Vec3ElemConverter(int elemIdx, object src, PropertyInfo prop)
        {
            Debug.Assert(elemIdx == 0 || elemIdx == 1 || elemIdx == 2, "Invalid index for Vec2ElemConverter");
            elemIndex = elemIdx;
            srcObject = src;
            property = prop;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is Vector3)
            {
                switch (elemIndex)
                {
                case 0:
                    return ((Vector3)value).X;
                case 1:
                    return ((Vector3)value).Y;
                case 2:
                    return ((Vector3)value).Z;
                }
            }
            return 0.0f;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            float val = 0.0f;
            if (float.TryParse(value.ToString(), out val))
            {
                Vector3 curValue = (Vector3)property.GetValue(srcObject);
                switch (elemIndex)
                {
                    case 0:
                        curValue.X = val;
                        break;
                    case 1:
                        curValue.Y = val;
                        break;
                    case 2:
                        curValue.Z = val;
                        break;
                }
                return curValue;
            }
            return value;
        }
    }

    /// <summary>
    /// Convert between a Vector4 and a string
    /// </summary>
    public class Vec4StrConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is Vector4)
            {
                return ((Vector4)value).ToString();
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = value.ToString();
            return DataExtensions.ParseVector4(str);
        }
    }

    /// <summary>
    /// CODE SIDE ONLY! Maps a vector element index to a binding.
    /// Requires a property and object for coherency
    /// </summary>
    public class Vec4ElemConverter : IValueConverter
    {
        int elemIndex = 0;
        PropertyInfo property;
        object srcObject;

        public Vec4ElemConverter(int elemIdx, object src, PropertyInfo prop)
        {
            Debug.Assert(elemIdx == 0 || elemIdx == 1 || elemIdx == 2 || elemIdx == 3, "Invalid index for Vec2ElemConverter");
            elemIndex = elemIdx;
            srcObject = src;
            property = prop;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is Vector4)
            {
                switch (elemIndex)
                {
                    case 0:
                        return ((Vector4)value).X;
                    case 1:
                        return ((Vector4)value).Y;
                    case 2:
                        return ((Vector4)value).Z;
                    case 3:
                        return ((Vector4)value).W;
                }
            }
            return 0.0f;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            float val = 0.0f;
            if (float.TryParse(value.ToString(), out val))
            {
                Vector4 curValue = (Vector4)property.GetValue(srcObject);
                switch (elemIndex)
                {
                    case 0:
                        curValue.X = val;
                        break;
                    case 1:
                        curValue.Y = val;
                        break;
                    case 2:
                        curValue.Z = val;
                        break;
                    case 3:
                        curValue.W = val;
                        break;
                }
                return curValue;
            }
            return new Vector4();
        }
    }

    public class Mat3x3ElemConverter : IValueConverter
    {
        int xIndex = 0;
        int yIndex = 0;
        PropertyInfo property;
        object srcObject;

        public Mat3x3ElemConverter(int xIdx, int yIdx, object src, PropertyInfo prop)
        {
            xIndex = xIdx;
            yIndex = yIdx;
            srcObject = src;
            property = prop;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is PluginLib.Mat3x3)
                return ((PluginLib.Mat3x3)value).m[xIndex, yIndex];
            return 0.0f;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            float val = 0.0f;
            if (float.TryParse(value.ToString(), out val))
            {
                PluginLib.Mat3x3 curValue = (PluginLib.Mat3x3)property.GetValue(srcObject);
                curValue.m[xIndex, yIndex] = val;
                return curValue;
            }
            return new Vector4();
        }
    }

    public class ColorElemConverter : IValueConverter
    {
        int elemIndex = 0;
        PropertyInfo property;
        object srcObject;

        public ColorElemConverter(int elemIdx, object src, PropertyInfo prop)
        {
            Debug.Assert(elemIdx == 0 || elemIdx == 1 || elemIdx == 2 || elemIdx == 3, "Invalid index for ColorElemConverter");
            elemIndex = elemIdx;
            srcObject = src;
            property = prop;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is Color)
            {
                switch (elemIndex)
                {
                    case 0:
                        return ((Color)value).R;
                    case 1:
                        return ((Color)value).G;
                    case 2:
                        return ((Color)value).B;
                    case 3:
                        return ((Color)value).A;
                }
            }
            return 0.0f;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            byte val = 0;
            if (byte.TryParse(value.ToString(), out val))
            {
                Color curValue = (Color)property.GetValue(srcObject);
                switch (elemIndex)
                {
                    case 0:
                        curValue.R = val;
                        break;
                    case 1:
                        curValue.G = val;
                        break;
                    case 2:
                        curValue.B = val;
                        break;
                    case 3:
                        curValue.A = val;
                        break;
                }
                return curValue;
            }
            return new Color();
        }
    }
}
