using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SprueKit.Data.Converters
{
    public class ExpanderConverter : IValueConverter
    {
        static Dictionary<string, bool> Values = new Dictionary<string, bool>
        {
            { "General", true }
        };

        public static bool IsExpanded(string key)
        {
            bool val = false;
            if (Values.TryGetValue(key, out val))
                return val;
            return false;
        }

        public static void SetExpanded(string key, bool state)
        {
            Values[key] = state;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string key = parameter.ToString();
            bool val = false;
            if (Values.TryGetValue(key, out val))
                return val;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Values[parameter.ToString()] = (bool)value;
            return value;
        }
    }
}
