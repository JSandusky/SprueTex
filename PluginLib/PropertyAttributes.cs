using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyData
{
    public enum GizmoProperties
    {
        [Description("All of scale, position, and rotation are allowed")]
        Full,
        [Description("Vector3 is treated as a direction only")]
        DirectionOnly,
        [Description("Vector3 or matrix is treated as position only")]
        PositionOnly,
        [Description("Only position and rotation of matrix will be used")]
        PositionRotationOnly,
    }

    public interface IDrawWithGizmo
    {

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class HasGizmoAttribute : System.Attribute
    {
        public GizmoProperties GizmoType { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class PreviewRequiresMesh : System.Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class NoPreviewsAttribute : System.Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class HelpNoChannelsAttribute : System.Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class GUIMethodAttribute : System.Attribute
    {
        public string Name { get; set; } = "";
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class OverrideNameAttribute : System.Attribute
    {
        public string Target { get; set; }
        public string NewName { get; set; }

        public OverrideNameAttribute(string target, string newName)
        {
            Target = target;
            NewName = newName;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    [Description("Used to mark what is a valid incremental step for this property")]
    public class ValidStepAttribute : System.Attribute
    {
        public float Value { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    [Description("If attached to a property then that property is permutable")]
    public class AllowPermutationsAttribute : System.Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
    [Description("Marks a field to be ignored in property reflection. \"propName\" parameter is only required when placed on a class to specify the target field.")]
    public class PropertyIgnoreAttribute : System.Attribute
    {
        public PropertyIgnoreAttribute(string propName = "")
        {
            PropName = propName;
        }

        public string PropName { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
    [Description("Marks a property to be displayed as readonly. \"propName\" parameter is only required when placed on a class to specify the target field.")]
    public class PropertyReadOnlyAttribute : System.Attribute
    {
        public PropertyReadOnlyAttribute(string propName = "")
        {
            PropName = propName;
        }

        public string PropName { get; set; }
    }


    [AttributeUsage(AttributeTargets.Property)]
    [Description("Marks an attribute as being for a file list instead of a folder list")]
    public class IsFileListAttribute : System.Attribute
    {
        public string Filter { get; set; }

        public IsFileListAttribute(string filter)
        {
            Filter = filter;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
    [Description("Marks the sort sequence of the property. \"propName\" parameter is only required when placed on a class to specify the target field.")]
    public class PropertyPriorityAttribute : System.Attribute
    {
        public PropertyPriorityAttribute(int level, string propName = "")
        {
            Level = level;
            PropertyName = propName;
        }

        public int Level { get; set; }
        public string PropertyName { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    [Description("Indicates which set of custom flags is used for this property.")]
    public class PropertyFlagsAttribute : System.Attribute
    {
        public string BitNames { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
    [Description("Marks a property as belonging to a specific group. \"propName\" parameter is only required when placed on a class to specify the target field, allows moving properties around in derived types (ie. advanced fields).")]
    public class PropertyGroupAttribute : System.Attribute
    {
        public PropertyGroupAttribute(string grpName, string propName = "")
        {
            GroupName = grpName;
            PropertyName = propName;
        }

        public string GroupName { get; set; }
        public string PropertyName { get; set; }
    }

    public enum VisualStage
    {
        None,
        Geometric,
        Rigging,
        Textural
    }

    [AttributeUsage(AttributeTargets.Property)]
    [Description("Use to mark properties that have consequences on the visual results of a document.")]
    public class VisualConsequenceAttribute : System.Attribute
    {
        public VisualStage Stage { get; set; } = VisualStage.Geometric;

        public VisualConsequenceAttribute(VisualStage stage = VisualStage.Geometric)
        {
            Stage = stage;
        }
    }

    public enum ResourceTagType
    {
        ForeignModel,
        SkeletalAnimation,
        RasterTexture,
        SVGTexture
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Description("Marks the underlying resource type of a Uri property. Used for selecting the appropriate file-open settings and determining if any custom controls are needed for the editor.")]
    public class ResourceTagAttribute : Attribute
    {
        public ResourceTagType Type { get; set; }
    }
}
