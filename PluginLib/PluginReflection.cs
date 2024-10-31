using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginLib
{
    
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    [Description("Marks a property to be ignored in automatic property display, use PropertyName when attaching to a class")]
    public class PropertyIgnoreAttribute : System.Attribute
    {
        public PropertyIgnoreAttribute()
        {

        }

        [Description("When placed on a class this field is used to determine which property to ignore.")]
        public string PropertyName { get; set; }
    }

    /// <summary>
    /// Indicates a method
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    [Description("Indicates a method to be invoked when the marked property is changed")]
    public class PropertyConsequenceAttribute : System.Attribute
    {
        public PropertyConsequenceAttribute(string methodName)
        {
            MethodName = methodName;
        }

        [Description("Fully qualified method name to invoke")]
        public string MethodName { get; set; }

        [Description("Indicates that the method to be invoked is static and not an instance method")]
        public bool IsStatic { get; set; } = true;
    }

    [Description("Overrides labeling for the marked property")]
    public class PropertyLabelAttribute : System.Attribute
    {
        public PropertyLabelAttribute(string lbl)
        {
            Label = lbl;
        }

        [Description("Text to display instead of the automatic label")]
        public string Label { get; set; }
    }
}
