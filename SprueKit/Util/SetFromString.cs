using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PluginLib;
using Microsoft.Xna.Framework;

namespace SprueKit.Util
{
    public static class SetFromString
    {
        public static bool Set(object target, string property, string valueString)
        {
            var prop = target.GetType().GetProperty(property);
            if (prop != null)
            {
                if (prop.PropertyType == typeof(bool))
                {
                    if (valueString.ToLowerInvariant().Equals("true") || valueString.ToLowerInvariant().Equals("1"))
                        prop.SetValue(target, true);
                    else
                        prop.SetValue(target, false);
                    return true;
                }
                else if (prop.PropertyType == typeof(int))
                {
                    int d = 0;
                    if (int.TryParse(valueString, out d))
                    {
                        prop.SetValue(target, d);
                        return true;
                    }
                }
                else if (prop.PropertyType == typeof(uint))
                {
                    uint d = 0;
                    if (uint.TryParse(valueString, out d))
                    {
                        prop.SetValue(target, d);
                        return true;
                    }
                }
                else if (prop.PropertyType == typeof(float))
                {
                    float d = 0;
                    if (float.TryParse(valueString, out d))
                    {
                        prop.SetValue(target, d);
                        return true;
                    }
                }
                else if (prop.PropertyType == typeof(double))
                {
                    double d = 0;
                    if (double.TryParse(valueString, out d))
                    {
                        prop.SetValue(target, d);
                        return true;
                    }
                }
                else if (prop.PropertyType == typeof(string))
                {
                    prop.SetValue(target, valueString);
                    return true;
                }
                else if (prop.PropertyType == typeof(Vector2))
                {
                    prop.SetValue(target, valueString.ToVector2());
                    return true;
                }
                else if (prop.PropertyType == typeof(Vector3))
                {
                    prop.SetValue(target, valueString.ToVector4());
                    return true;
                }
                else if (prop.PropertyType == typeof(Vector4))
                {
                    prop.SetValue(target, valueString.ToVector4());
                    return true;
                }
                else if (prop.PropertyType == typeof(Color))
                {
                    prop.SetValue(target, valueString.ToColor());
                    return true;
                }
                else if (prop.PropertyType == typeof(IntVector2))
                {
                    prop.SetValue(target, valueString.ToIntVector2());
                    return true;
                }
                else if (prop.PropertyType == typeof(IntVector4))
                {
                    prop.SetValue(target, valueString.ToIntVector4());
                    return true;
                }
                else if (prop.PropertyType == typeof(Uri))
                {
                    if (System.IO.File.Exists(valueString))
                    {
                        prop.SetValue(target, new Uri(valueString));
                        return true;
                    }
                }
                else if (prop.PropertyType == typeof(Data.ForeignModel))
                {
                    if (System.IO.File.Exists(valueString))
                    {
                        ((Data.ForeignModel)prop.GetValue(target)).ModelFile = new Uri(valueString);
                        return ((Data.ForeignModel)prop.GetValue(target)).ModelData != null;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Traces through to the tail of an object
        /// </summary>
        public static object TraceValue(object origin, string[] properties)
        {
            if (origin == null)
                return null;
            if (properties.Length == 1)
                return origin;
            var obj = origin.GetType().GetProperty(properties[0]);
            if (obj != null)
                return TraceValue(obj, properties.SubArray(1, properties.Length - 1));
            return null;
        }
    }
}
