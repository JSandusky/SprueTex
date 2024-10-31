using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;
using Color = Microsoft.Xna.Framework.Color;

namespace SprueKit.Util
{
    public static class TypeRandomizer
    {
        public static void Randomize(object target)
        {
            Random r = new Random(target.GetHashCode() * DateTime.Now.Millisecond);
            foreach (var pi in target.GetType().GetProperties())
            {
                var validStep = pi.GetCustomAttribute<PropertyData.ValidStepAttribute>();
                if (validStep == null || pi.PropertyType.IsEnum || pi.PropertyType == typeof(bool))
                    continue;
                if (pi.PropertyType == typeof(float))
                {
                    float step = validStep.Value;
                    float iValue = (float)pi.GetValue(target);
                    pi.SetValue(target, iValue + (float)(r.NextDouble() * step));
                }
                else if (pi.PropertyType == typeof(int))
                {
                    float step = validStep.Value;
                    int iValue = (int)pi.GetValue(target);
                    pi.SetValue(target, r.Next(iValue, (int)(iValue + step)));
                }
                else if (pi.PropertyType == typeof(Vector2))
                {
                    float step = validStep.Value;
                    Vector2 iValue = (Vector2)pi.GetValue(target);
                    iValue.X += (float)(r.NextDouble() * step);
                    iValue.Y += (float)(r.NextDouble() * step);
                    pi.SetValue(target, iValue);
                }
                else if (pi.PropertyType == typeof(Vector3))
                {
                    float step = validStep.Value;
                    Vector3 iValue = (Vector3)pi.GetValue(target);
                    iValue.X += (float)(r.NextDouble() * step);
                    iValue.Y += (float)(r.NextDouble() * step);
                    iValue.Z += (float)(r.NextDouble() * step);
                    pi.SetValue(target, iValue);
                }
                else if (pi.PropertyType == typeof(Vector4))
                {
                    float step = validStep.Value;
                    Vector4 iValue = (Vector4)pi.GetValue(target);
                    iValue.X += (float)(r.NextDouble() * step);
                    iValue.Y += (float)(r.NextDouble() * step);
                    iValue.Z += (float)(r.NextDouble() * step);
                    iValue.W += (float)(r.NextDouble() * step);
                    pi.SetValue(target, iValue);
                }
                else if (pi.PropertyType == typeof(Color))
                {
                    pi.SetValue(target, new Color(r.Next(255), r.Next(255), r.Next(255), 255));
                }
                else if (pi.PropertyType.IsEnum)
                {
                    var enumValues = Enum.GetValues(pi.PropertyType);
                    pi.SetValue(target, enumValues.GetValue(r.Next(enumValues.Length)));
                }
                else if (pi.PropertyType == typeof(bool))
                {
                    pi.SetValue(target, r.Next() % 2 != 0);
                }
            }
        }
    }
}
