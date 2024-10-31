using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Reflection;
using SprueKit.Commands;

namespace SprueKit.Data.Converters
{
    public class UndoRedoConverter : IValueConverter
    {
        object who;
        PropertyInfo property;
        UndoStack stack;

        UndoRedoConverter(PropertyInfo property, object who, UndoStack stack)
        {
            this.who = who;
            this.property = property;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            stack.Add(new BasicPropertyCmd(property.Name, who, property.GetValue(who), value));
            return value;
        }
    }
}
