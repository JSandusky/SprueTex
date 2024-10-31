using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SprueKit;
using SprueKit.Data;
using System.IO;
using Color = Microsoft.Xna.Framework.Color;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;
using IntVector2 = PluginLib.IntVector2;
using PluginLib;

namespace SprueKit.Data.TexGen
{
    public partial class FloatNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Value", Value.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Value = thisElem.GetFloatElement("Value");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Value);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Value = strm.ReadSingle();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            FloatNode retVal = new FloatNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            FloatNode retVal = into as FloatNode;
            retVal.value_ = this.value_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class ColorNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Color", Color.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Color = thisElem.GetStringElement("Color").ToColor();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Color);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Color = strm.ReadColor();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            ColorNode retVal = new ColorNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            ColorNode retVal = into as ColorNode;
            retVal.color_ = this.color_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class TextureNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            if (Texture != null && System.IO.File.Exists(Texture.AbsolutePath))
                thisElem.AddStringElement("Texture", ctx.GetRelativePathString(Texture));
            thisElem.AddStringElement("BilinearFilter", BilinearFilter.ToString());
            thisElem.AddStringElement("GradientFilter", GradientFilter.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            {
                string fileString = thisElem.GetStringElement("Texture");
                if (!string.IsNullOrWhiteSpace(fileString))
                {
                    Uri result = ctx.GetAbsolutePath(new Uri(thisElem.GetStringElement(fileString)), this, "TextureNode", "TextureNode", FileData.ImageFileMask);
                    if (result != null) Texture = result;
                }
            }
            BilinearFilter = thisElem.GetBoolElement("BilinearFilter");
            GradientFilter = thisElem.GetBoolElement("GradientFilter");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            if (Texture != null && System.IO.File.Exists(Texture.AbsolutePath))
            {
                strm.Write(true);
                strm.Write(ctx.GetRelativePathString(Texture));
            }
            else strm.Write(false);
            strm.Write(BilinearFilter);
            strm.Write(GradientFilter);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            if (strm.ReadBoolean())
            {
                Uri result = ctx.GetAbsolutePath(new Uri(strm.ReadString()), this, "TextureNode", "TextureNode", FileData.ImageFileMask);
                if (result != null) Texture = result;
            }
            BilinearFilter = strm.ReadBoolean();
            GradientFilter = strm.ReadBoolean();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            TextureNode retVal = new TextureNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            TextureNode retVal = into as TextureNode;
            retVal.data_ = this.data_;
            retVal.imageFile_ = this.imageFile_;
            retVal.bilinearFilter_ = this.bilinearFilter_;
            retVal.gradientFilter_ = this.gradientFilter_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class IDMapGenerator
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            if (Texture != null && System.IO.File.Exists(Texture.AbsolutePath))
                thisElem.AddStringElement("Texture", ctx.GetRelativePathString(Texture));
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            {
                string fileString = thisElem.GetStringElement("Texture");
                if (!string.IsNullOrWhiteSpace(fileString))
                {
                    Uri result = ctx.GetAbsolutePath(new Uri(thisElem.GetStringElement(fileString)), this, "IDMapGenerator", "IDMapGenerator", FileData.ImageFileMask);
                    if (result != null) Texture = result;
                }
            }
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            if (Texture != null && System.IO.File.Exists(Texture.AbsolutePath))
            {
                strm.Write(true);
                strm.Write(ctx.GetRelativePathString(Texture));
            }
            else strm.Write(false);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            if (strm.ReadBoolean())
            {
                Uri result = ctx.GetAbsolutePath(new Uri(strm.ReadString()), this, "IDMapGenerator", "IDMapGenerator", FileData.ImageFileMask);
                if (result != null) Texture = result;
            }
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            IDMapGenerator retVal = new IDMapGenerator();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            IDMapGenerator retVal = into as IDMapGenerator;
            retVal.data_ = this.data_;
            retVal.colors_ = this.colors_;
            retVal.imageFile_ = this.imageFile_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class SVGNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Width", Width.ToString());
            thisElem.AddStringElement("Height", Height.ToString());
            if (Texture != null && System.IO.File.Exists(Texture.AbsolutePath))
                thisElem.AddStringElement("Texture", ctx.GetRelativePathString(Texture));
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Width = thisElem.GetIntElement("Width");
            Height = thisElem.GetIntElement("Height");
            {
                string fileString = thisElem.GetStringElement("Texture");
                if (!string.IsNullOrWhiteSpace(fileString))
                {
                    Uri result = ctx.GetAbsolutePath(new Uri(thisElem.GetStringElement(fileString)), this, "SVGNode", "SVGNode", FileData.SVGFileMask);
                    if (result != null) Texture = result;
                }
            }
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Width);
            strm.Write(Height);
            if (Texture != null && System.IO.File.Exists(Texture.AbsolutePath))
            {
                strm.Write(true);
                strm.Write(ctx.GetRelativePathString(Texture));
            }
            else strm.Write(false);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Width = strm.ReadInt32();
            Height = strm.ReadInt32();
            if (strm.ReadBoolean())
            {
                Uri result = ctx.GetAbsolutePath(new Uri(strm.ReadString()), this, "SVGNode", "SVGNode", FileData.SVGFileMask);
                if (result != null) Texture = result;
            }
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            SVGNode retVal = new SVGNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            SVGNode retVal = into as SVGNode;
            retVal.width_ = this.width_;
            retVal.height_ = this.height_;
            retVal.data_ = this.data_;
            retVal.imageFile_ = this.imageFile_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class ModelNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            if (ModelFile != null && ModelFile.ModelFile != null)
            {
                var mdl = thisElem.CreateChild("ModelFile");
                ModelFile.Write(ctx, mdl);
            }
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            {
                var mdlElem = thisElem.SelectSingleNode("ModelFile") as XmlElement;
                if (mdlElem != null) ModelFile.Read(ctx, mdlElem);
            }
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            if (ModelFile != null && ModelFile.ModelFile != null)
            {
                strm.Write(true);
                ModelFile.Write(ctx, strm);
            }
            else strm.Write(false);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            if (strm.ReadBoolean())
            {
                ModelFile.Read(ctx, strm);
            }
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            ModelNode retVal = new ModelNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            ModelNode retVal = into as ModelNode;
            retVal.modelFile_ = this.modelFile_;
            retVal.dirty_ = this.dirty_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class PBREnforcer
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("AlertMode", AlertMode.ToString());
            thisElem.AddStringElement("StrictMode", StrictMode.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            AlertMode = thisElem.GetBoolElement("AlertMode");
            StrictMode = thisElem.GetBoolElement("StrictMode");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(AlertMode);
            strm.Write(StrictMode);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            AlertMode = strm.ReadBoolean();
            StrictMode = strm.ReadBoolean();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            PBREnforcer retVal = new PBREnforcer();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            PBREnforcer retVal = into as PBREnforcer;
            retVal.alertMode_ = this.alertMode_;
            retVal.strictMode_ = this.strictMode_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class CombineNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            CombineNode retVal = new CombineNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            CombineNode retVal = into as CombineNode;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class SplitNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            SplitNode retVal = new SplitNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            SplitNode retVal = into as SplitNode;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class AddNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            AddNode retVal = new AddNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            AddNode retVal = into as AddNode;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class SubtractNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            SubtractNode retVal = new SubtractNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            SubtractNode retVal = into as SubtractNode;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class MultiplyNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            MultiplyNode retVal = new MultiplyNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            MultiplyNode retVal = into as MultiplyNode;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class DivideNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            DivideNode retVal = new DivideNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            DivideNode retVal = into as DivideNode;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class BricksGenerator
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("BaseOffset", BaseOffset.ToTightString());
            thisElem.AddStringElement("TileSize", TileSize.ToTightString());
            thisElem.AddStringElement("RowOffset", RowOffset.ToString());
            thisElem.AddStringElement("Gutter", Gutter.ToTightString());
            thisElem.AddStringElement("PerturbPower", PerturbPower.ToTightString());
            thisElem.AddStringElement("BlockColor", BlockColor.ToTightString());
            thisElem.AddStringElement("GroutColor", GroutColor.ToTightString());
            thisElem.AddStringElement("RandomizeBlocks", RandomizeBlocks.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Description = thisElem.GetStringElement("Description");
            BaseOffset = thisElem.GetStringElement("BaseOffset").ToVector2();
            TileSize = thisElem.GetStringElement("TileSize").ToVector2();
            RowOffset = thisElem.GetFloatElement("RowOffset");
            Gutter = thisElem.GetStringElement("Gutter").ToVector2();
            PerturbPower = thisElem.GetStringElement("PerturbPower").ToVector2();
            BlockColor = thisElem.GetStringElement("BlockColor").ToColor();
            GroutColor = thisElem.GetStringElement("GroutColor").ToColor();
            RandomizeBlocks = thisElem.GetBoolElement("RandomizeBlocks");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Description ?? "");
            strm.Write(BaseOffset);
            strm.Write(TileSize);
            strm.Write(RowOffset);
            strm.Write(Gutter);
            strm.Write(PerturbPower);
            strm.Write(BlockColor);
            strm.Write(GroutColor);
            strm.Write(RandomizeBlocks);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Description = strm.ReadString();
            BaseOffset = strm.ReadVector2();
            TileSize = strm.ReadVector2();
            RowOffset = strm.ReadSingle();
            Gutter = strm.ReadVector2();
            PerturbPower = strm.ReadVector2();
            BlockColor = strm.ReadColor();
            GroutColor = strm.ReadColor();
            RandomizeBlocks = strm.ReadBoolean();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            BricksGenerator retVal = new BricksGenerator();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            BricksGenerator retVal = into as BricksGenerator;
            retVal.tileSize_ = this.tileSize_;
            retVal.rowOffset_ = this.rowOffset_;
            retVal.baseOffset_ = this.baseOffset_;
            retVal.gutter_ = this.gutter_;
            retVal.perturbPower_ = this.perturbPower_;
            retVal.blockColor_ = this.blockColor_;
            retVal.groutColor_ = this.groutColor_;
            retVal.randomizeBlocks_ = this.randomizeBlocks_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class ChainGenerator
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("BackgroundColor", BackgroundColor.ToTightString());
            thisElem.AddStringElement("CenterLinkColor", CenterLinkColor.ToTightString());
            thisElem.AddStringElement("ConnectingLinkColor", ConnectingLinkColor.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            BackgroundColor = thisElem.GetStringElement("BackgroundColor").ToColor();
            CenterLinkColor = thisElem.GetStringElement("CenterLinkColor").ToColor();
            ConnectingLinkColor = thisElem.GetStringElement("ConnectingLinkColor").ToColor();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(BackgroundColor);
            strm.Write(CenterLinkColor);
            strm.Write(ConnectingLinkColor);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            BackgroundColor = strm.ReadColor();
            CenterLinkColor = strm.ReadColor();
            ConnectingLinkColor = strm.ReadColor();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            ChainGenerator retVal = new ChainGenerator();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            ChainGenerator retVal = into as ChainGenerator;
            retVal.backgroundColor_ = this.backgroundColor_;
            retVal.centerLinkColor_ = this.centerLinkColor_;
            retVal.connectingLinkColor_ = this.connectingLinkColor_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class ChainMailGenerator
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("BackgroundColor", BackgroundColor.ToTightString());
            thisElem.AddStringElement("CenterLinkColor", CenterLinkColor.ToTightString());
            thisElem.AddStringElement("ConnectingLinkColor", ConnectingLinkColor.ToTightString());
            thisElem.AddStringElement("ChainSize", ChainSize.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            BackgroundColor = thisElem.GetStringElement("BackgroundColor").ToColor();
            CenterLinkColor = thisElem.GetStringElement("CenterLinkColor").ToColor();
            ConnectingLinkColor = thisElem.GetStringElement("ConnectingLinkColor").ToColor();
            ChainSize = thisElem.GetStringElement("ChainSize").ToVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(BackgroundColor);
            strm.Write(CenterLinkColor);
            strm.Write(ConnectingLinkColor);
            strm.Write(ChainSize);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            BackgroundColor = strm.ReadColor();
            CenterLinkColor = strm.ReadColor();
            ConnectingLinkColor = strm.ReadColor();
            ChainSize = strm.ReadVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            ChainMailGenerator retVal = new ChainMailGenerator();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            ChainMailGenerator retVal = into as ChainMailGenerator;
            retVal.backgroundColor_ = this.backgroundColor_;
            retVal.centerLinkColor_ = this.centerLinkColor_;
            retVal.connectingLinkColor_ = this.connectingLinkColor_;
            retVal.chainSize_ = this.chainSize_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class CheckerGenerator
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("ColorA", ColorA.ToTightString());
            thisElem.AddStringElement("ColorB", ColorB.ToTightString());
            thisElem.AddStringElement("TileCount", TileCount.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            ColorA = thisElem.GetStringElement("ColorA").ToColor();
            ColorB = thisElem.GetStringElement("ColorB").ToColor();
            TileCount = thisElem.GetStringElement("TileCount").ToIntVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(ColorA);
            strm.Write(ColorB);
            strm.Write(TileCount);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            ColorA = strm.ReadColor();
            ColorB = strm.ReadColor();
            TileCount = strm.ReadIntVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            CheckerGenerator retVal = new CheckerGenerator();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            CheckerGenerator retVal = into as CheckerGenerator;
            retVal.colorA_ = this.colorA_;
            retVal.colorB_ = this.colorB_;
            retVal.tileCount_ = this.tileCount_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class FBMNoiseGenerator
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Fractal", Fractal.ToString());
            thisElem.AddStringElement("Gain", Gain.ToString());
            thisElem.AddStringElement("Lacunarity", Lacunarity.ToString());
            thisElem.AddStringElement("Octaves", Octaves.ToString());
            thisElem.AddStringElement("Seed", Seed.ToString());
            thisElem.AddStringElement("Inverted", Inverted.ToString());
            thisElem.AddStringElement("Period", Period.ToTightString());
            thisElem.AddStringElement("Interpolation", Interpolation.ToString());
            thisElem.AddStringElement("Frequency", Frequency.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Fractal = (FastNoise.FractalType)Enum.Parse(typeof(FastNoise.FractalType), thisElem.GetStringElement("Fractal"));
            Gain = thisElem.GetFloatElement("Gain");
            Lacunarity = thisElem.GetFloatElement("Lacunarity");
            Octaves = thisElem.GetIntElement("Octaves");
            Seed = thisElem.GetIntElement("Seed");
            Inverted = thisElem.GetBoolElement("Inverted");
            Period = thisElem.GetStringElement("Period").ToVector2();
            Interpolation = (FastNoise.Interp)Enum.Parse(typeof(FastNoise.Interp), thisElem.GetStringElement("Interpolation"));
            Frequency = thisElem.GetFloatElement("Frequency");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write((int)Fractal);
            strm.Write(Gain);
            strm.Write(Lacunarity);
            strm.Write(Octaves);
            strm.Write(Seed);
            strm.Write(Inverted);
            strm.Write(Period);
            strm.Write((int)Interpolation);
            strm.Write(Frequency);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Fractal = (FastNoise.FractalType)strm.ReadInt32();
            Gain = strm.ReadSingle();
            Lacunarity = strm.ReadSingle();
            Octaves = strm.ReadInt32();
            Seed = strm.ReadInt32();
            Inverted = strm.ReadBoolean();
            Period = strm.ReadVector2();
            Interpolation = (FastNoise.Interp)strm.ReadInt32();
            Frequency = strm.ReadSingle();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            FBMNoiseGenerator retVal = new FBMNoiseGenerator();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            FBMNoiseGenerator retVal = into as FBMNoiseGenerator;
            retVal.noise_ = this.noise_.Clone();
            retVal.inverted_ = this.inverted_;
            retVal.period_ = this.period_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class GaborNoise
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Alpha", Alpha.ToString());
            thisElem.AddStringElement("Impulses", Impulses.ToString());
            thisElem.AddStringElement("K", K.ToString());
            thisElem.AddStringElement("F0", F0.ToString());
            thisElem.AddStringElement("Omega", Omega.ToString());
            thisElem.AddStringElement("Seed", Seed.ToString());
            thisElem.AddStringElement("Inverted", Inverted.ToString());
            thisElem.AddStringElement("Period", Period.ToTightString());
            thisElem.AddStringElement("Interpolation", Interpolation.ToString());
            thisElem.AddStringElement("Frequency", Frequency.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Alpha = thisElem.GetFloatElement("Alpha");
            Impulses = thisElem.GetFloatElement("Impulses");
            K = thisElem.GetFloatElement("K");
            F0 = thisElem.GetFloatElement("F0");
            Omega = thisElem.GetFloatElement("Omega");
            Seed = thisElem.GetIntElement("Seed");
            Inverted = thisElem.GetBoolElement("Inverted");
            Period = thisElem.GetStringElement("Period").ToVector2();
            Interpolation = (FastNoise.Interp)Enum.Parse(typeof(FastNoise.Interp), thisElem.GetStringElement("Interpolation"));
            Frequency = thisElem.GetFloatElement("Frequency");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Alpha);
            strm.Write(Impulses);
            strm.Write(K);
            strm.Write(F0);
            strm.Write(Omega);
            strm.Write(Seed);
            strm.Write(Inverted);
            strm.Write(Period);
            strm.Write((int)Interpolation);
            strm.Write(Frequency);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Alpha = strm.ReadSingle();
            Impulses = strm.ReadSingle();
            K = strm.ReadSingle();
            F0 = strm.ReadSingle();
            Omega = strm.ReadSingle();
            Seed = strm.ReadInt32();
            Inverted = strm.ReadBoolean();
            Period = strm.ReadVector2();
            Interpolation = (FastNoise.Interp)strm.ReadInt32();
            Frequency = strm.ReadSingle();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            GaborNoise retVal = new GaborNoise();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            GaborNoise retVal = into as GaborNoise;
            retVal.alpha_ = this.alpha_;
            retVal.impules_ = this.impules_;
            retVal.k_ = this.k_;
            retVal.f0_ = this.f0_;
            retVal.omega_ = this.omega_;
            retVal.offset_ = this.offset_;
            retVal.noise_ = this.noise_.Clone();
            retVal.inverted_ = this.inverted_;
            retVal.period_ = this.period_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class GradientGenerator
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Offset", Offset.ToTightString());
            thisElem.AddStringElement("Start", Start.ToTightString());
            thisElem.AddStringElement("End", End.ToTightString());
            thisElem.AddStringElement("Length", Length.ToString());
            thisElem.AddStringElement("Angle", Angle.ToString());
            thisElem.AddStringElement("Type", Type.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Offset = thisElem.GetStringElement("Offset").ToVector2();
            Start = thisElem.GetStringElement("Start").ToColor();
            End = thisElem.GetStringElement("End").ToColor();
            Length = thisElem.GetFloatElement("Length");
            Angle = thisElem.GetFloatElement("Angle");
            Type = (SprueKit.Data.TexGen.GradientGenerator.GradientType)Enum.Parse(typeof(SprueKit.Data.TexGen.GradientGenerator.GradientType), thisElem.GetStringElement("Type"));
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Offset);
            strm.Write(Start);
            strm.Write(End);
            strm.Write(Length);
            strm.Write(Angle);
            strm.Write((int)Type);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Offset = strm.ReadVector2();
            Start = strm.ReadColor();
            End = strm.ReadColor();
            Length = strm.ReadSingle();
            Angle = strm.ReadSingle();
            Type = (SprueKit.Data.TexGen.GradientGenerator.GradientType)strm.ReadInt32();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            GradientGenerator retVal = new GradientGenerator();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            GradientGenerator retVal = into as GradientGenerator;
            retVal.offset_ = this.offset_;
            retVal.start_ = this.start_;
            retVal.end_ = this.end_;
            retVal.length_ = this.length_;
            retVal.angle_ = this.angle_;
            retVal.gradientType_ = this.gradientType_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class PerlinNoiseGenerator
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Seed", Seed.ToString());
            thisElem.AddStringElement("Inverted", Inverted.ToString());
            thisElem.AddStringElement("Period", Period.ToTightString());
            thisElem.AddStringElement("Interpolation", Interpolation.ToString());
            thisElem.AddStringElement("Frequency", Frequency.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Seed = thisElem.GetIntElement("Seed");
            Inverted = thisElem.GetBoolElement("Inverted");
            Period = thisElem.GetStringElement("Period").ToVector2();
            Interpolation = (FastNoise.Interp)Enum.Parse(typeof(FastNoise.Interp), thisElem.GetStringElement("Interpolation"));
            Frequency = thisElem.GetFloatElement("Frequency");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Seed);
            strm.Write(Inverted);
            strm.Write(Period);
            strm.Write((int)Interpolation);
            strm.Write(Frequency);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Seed = strm.ReadInt32();
            Inverted = strm.ReadBoolean();
            Period = strm.ReadVector2();
            Interpolation = (FastNoise.Interp)strm.ReadInt32();
            Frequency = strm.ReadSingle();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            PerlinNoiseGenerator retVal = new PerlinNoiseGenerator();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            PerlinNoiseGenerator retVal = into as PerlinNoiseGenerator;
            retVal.noise_ = this.noise_.Clone();
            retVal.inverted_ = this.inverted_;
            retVal.period_ = this.period_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class PolygonGenerator
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("SideCount", SideCount.ToString());
            thisElem.AddStringElement("Rotate", Rotate.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            SideCount = thisElem.GetIntElement("SideCount");
            Rotate = thisElem.GetFloatElement("Rotate");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(SideCount);
            strm.Write(Rotate);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            SideCount = strm.ReadInt32();
            Rotate = strm.ReadSingle();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            PolygonGenerator retVal = new PolygonGenerator();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            PolygonGenerator retVal = into as PolygonGenerator;
            retVal.sideCount_ = this.sideCount_;
            retVal.rotate_ = this.rotate_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class RowsGenerator
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("RowCount", RowCount.ToString());
            thisElem.AddStringElement("PerturbationPower", PerturbationPower.ToString());
            thisElem.AddStringElement("Vertical", Vertical.ToString());
            thisElem.AddStringElement("AlternateDeadColumns", AlternateDeadColumns.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            RowCount = thisElem.GetIntElement("RowCount");
            PerturbationPower = thisElem.GetFloatElement("PerturbationPower");
            Vertical = thisElem.GetBoolElement("Vertical");
            AlternateDeadColumns = thisElem.GetBoolElement("AlternateDeadColumns");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(RowCount);
            strm.Write(PerturbationPower);
            strm.Write(Vertical);
            strm.Write(AlternateDeadColumns);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            RowCount = strm.ReadInt32();
            PerturbationPower = strm.ReadSingle();
            Vertical = strm.ReadBoolean();
            AlternateDeadColumns = strm.ReadBoolean();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            RowsGenerator retVal = new RowsGenerator();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            RowsGenerator retVal = into as RowsGenerator;
            retVal.rowCount_ = this.rowCount_;
            retVal.perturbPower_ = this.perturbPower_;
            retVal.vertical_ = this.vertical_;
            retVal.alternateDeadColumns_ = this.alternateDeadColumns_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class ScalesGenerator
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("EvenColor", EvenColor.ToTightString());
            thisElem.AddStringElement("OddColor", OddColor.ToTightString());
            thisElem.AddStringElement("ScaleSize", ScaleSize.ToTightString());
            thisElem.AddStringElement("Matched", Matched.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            EvenColor = thisElem.GetStringElement("EvenColor").ToColor();
            OddColor = thisElem.GetStringElement("OddColor").ToColor();
            ScaleSize = thisElem.GetStringElement("ScaleSize").ToVector2();
            Matched = thisElem.GetBoolElement("Matched");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(EvenColor);
            strm.Write(OddColor);
            strm.Write(ScaleSize);
            strm.Write(Matched);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            EvenColor = strm.ReadColor();
            OddColor = strm.ReadColor();
            ScaleSize = strm.ReadVector2();
            Matched = strm.ReadBoolean();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            ScalesGenerator retVal = new ScalesGenerator();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            ScalesGenerator retVal = into as ScalesGenerator;
            retVal.evenColor_ = this.evenColor_;
            retVal.oddColor_ = this.oddColor_;
            retVal.scaleSize_ = this.scaleSize_;
            retVal.matched_ = this.matched_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class ScratchesGenerator
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Seed", Seed.ToString());
            thisElem.AddStringElement("Density", Density.ToString());
            thisElem.AddStringElement("Length", Length.ToString());
            thisElem.AddStringElement("Inverted", Inverted.ToString());
            thisElem.AddStringElement("FadeOff", FadeOff.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Seed = thisElem.GetIntElement("Seed");
            Density = thisElem.GetIntElement("Density");
            Length = thisElem.GetFloatElement("Length");
            Inverted = thisElem.GetBoolElement("Inverted");
            FadeOff = thisElem.GetBoolElement("FadeOff");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Seed);
            strm.Write(Density);
            strm.Write(Length);
            strm.Write(Inverted);
            strm.Write(FadeOff);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Seed = strm.ReadInt32();
            Density = strm.ReadInt32();
            Length = strm.ReadSingle();
            Inverted = strm.ReadBoolean();
            FadeOff = strm.ReadBoolean();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            ScratchesGenerator retVal = new ScratchesGenerator();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            ScratchesGenerator retVal = into as ScratchesGenerator;
            retVal.noise_ = this.noise_.Clone();
            retVal.density_ = this.density_;
            retVal.length_ = this.length_;
            retVal.inverted_ = this.inverted_;
            retVal.fadeOff_ = this.fadeOff_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class TextGenerator
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Text", Text);
            thisElem.AddStringElement("Font", Font.ToString()); thisElem.AddStringElement("Color", Color.ToTightString());
            thisElem.AddStringElement("HorizontalAlignment", HorizontalAlignment.ToString());
            thisElem.AddStringElement("VerticalAlignment", VerticalAlignment.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Text = thisElem.GetStringElement("Text");
            Font = Data.FontSpec.FromString(thisElem.GetStringElement("Font")); Color = thisElem.GetStringElement("Color").ToColor();
            HorizontalAlignment = (SprueKit.Data.TexGen.TextHorizontalAlignment)Enum.Parse(typeof(SprueKit.Data.TexGen.TextHorizontalAlignment), thisElem.GetStringElement("HorizontalAlignment"));
            VerticalAlignment = (SprueKit.Data.TexGen.TextVerticalAlignment)Enum.Parse(typeof(SprueKit.Data.TexGen.TextVerticalAlignment), thisElem.GetStringElement("VerticalAlignment"));
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Text ?? "");
            strm.Write(Font); strm.Write(Color);
            strm.Write((int)HorizontalAlignment);
            strm.Write((int)VerticalAlignment);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Text = strm.ReadString();
            Font = strm.ReadFontSpec(); Color = strm.ReadColor();
            HorizontalAlignment = (SprueKit.Data.TexGen.TextHorizontalAlignment)strm.ReadInt32();
            VerticalAlignment = (SprueKit.Data.TexGen.TextVerticalAlignment)strm.ReadInt32();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            TextGenerator retVal = new TextGenerator();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            TextGenerator retVal = into as TextGenerator;
            retVal.cache_ = this.cache_;
            retVal.font_ = this.font_.Clone();
            retVal.color_ = this.color_;
            retVal.text_ = this.text_;
            retVal.horizontal_ = this.horizontal_;
            retVal.vertical_ = this.vertical_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class TextureBombGenerator
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Count", Count.ToString());
            thisElem.AddStringElement("ScaleRange", ScaleRange.ToTightString());
            thisElem.AddStringElement("RotationRange", RotationRange.ToTightString());
            thisElem.AddStringElement("Seed", Seed.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Count = thisElem.GetIntElement("Count");
            ScaleRange = thisElem.GetStringElement("ScaleRange").ToVector2();
            RotationRange = thisElem.GetStringElement("RotationRange").ToVector2();
            Seed = thisElem.GetIntElement("Seed");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Count);
            strm.Write(ScaleRange);
            strm.Write(RotationRange);
            strm.Write(Seed);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Count = strm.ReadInt32();
            ScaleRange = strm.ReadVector2();
            RotationRange = strm.ReadVector2();
            Seed = strm.ReadInt32();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            TextureBombGenerator retVal = new TextureBombGenerator();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            TextureBombGenerator retVal = into as TextureBombGenerator;
            retVal.noise_ = this.noise_.Clone();
            retVal.count_ = this.count_;
            retVal.scaleRange_ = this.scaleRange_;
            retVal.rotationRange_ = this.rotationRange_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class TextureFunction2D
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("Offset", Offset.ToTightString());
            thisElem.AddStringElement("Perturb", Perturb.ToTightString());
            thisElem.AddStringElement("Period", Period.ToTightString());
            thisElem.AddStringElement("XFunction", XFunction.ToString());
            thisElem.AddStringElement("YFunction", YFunction.ToString());
            thisElem.AddStringElement("DiagonalFunction", DiagonalFunction.ToString());
            thisElem.AddStringElement("Mix", Mix.ToString());
            thisElem.AddStringElement("DiagonalMix", DiagonalMix.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Description = thisElem.GetStringElement("Description");
            Offset = thisElem.GetStringElement("Offset").ToVector2();
            Perturb = thisElem.GetStringElement("Perturb").ToVector3();
            Period = thisElem.GetStringElement("Period").ToVector2();
            XFunction = (SprueKit.Data.TexGen.TextureFunction2D.TextureFunctionFunction)Enum.Parse(typeof(SprueKit.Data.TexGen.TextureFunction2D.TextureFunctionFunction), thisElem.GetStringElement("XFunction"));
            YFunction = (SprueKit.Data.TexGen.TextureFunction2D.TextureFunctionFunction)Enum.Parse(typeof(SprueKit.Data.TexGen.TextureFunction2D.TextureFunctionFunction), thisElem.GetStringElement("YFunction"));
            DiagonalFunction = (SprueKit.Data.TexGen.TextureFunction2D.TextureFunctionFunction)Enum.Parse(typeof(SprueKit.Data.TexGen.TextureFunction2D.TextureFunctionFunction), thisElem.GetStringElement("DiagonalFunction"));
            Mix = (SprueKit.Data.TexGen.TextureFunction2D.TextureFunctionMix)Enum.Parse(typeof(SprueKit.Data.TexGen.TextureFunction2D.TextureFunctionMix), thisElem.GetStringElement("Mix"));
            DiagonalMix = (SprueKit.Data.TexGen.TextureFunction2D.TextureFunctionMix)Enum.Parse(typeof(SprueKit.Data.TexGen.TextureFunction2D.TextureFunctionMix), thisElem.GetStringElement("DiagonalMix"));
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Description ?? "");
            strm.Write(Offset);
            strm.Write(Perturb);
            strm.Write(Period);
            strm.Write((int)XFunction);
            strm.Write((int)YFunction);
            strm.Write((int)DiagonalFunction);
            strm.Write((int)Mix);
            strm.Write((int)DiagonalMix);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Description = strm.ReadString();
            Offset = strm.ReadVector2();
            Perturb = strm.ReadVector3();
            Period = strm.ReadVector2();
            XFunction = (SprueKit.Data.TexGen.TextureFunction2D.TextureFunctionFunction)strm.ReadInt32();
            YFunction = (SprueKit.Data.TexGen.TextureFunction2D.TextureFunctionFunction)strm.ReadInt32();
            DiagonalFunction = (SprueKit.Data.TexGen.TextureFunction2D.TextureFunctionFunction)strm.ReadInt32();
            Mix = (SprueKit.Data.TexGen.TextureFunction2D.TextureFunctionMix)strm.ReadInt32();
            DiagonalMix = (SprueKit.Data.TexGen.TextureFunction2D.TextureFunctionMix)strm.ReadInt32();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            TextureFunction2D retVal = new TextureFunction2D();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            TextureFunction2D retVal = into as TextureFunction2D;
            retVal.xFunc_ = this.xFunc_;
            retVal.yFunc_ = this.yFunc_;
            retVal.diagFunc_ = this.diagFunc_;
            retVal.mix_ = this.mix_;
            retVal.diagMix_ = this.diagMix_;
            retVal.pertrub_ = this.pertrub_;
            retVal.period_ = this.period_;
            retVal.offset_ = this.offset_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class UberNoise
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Lacunarity", Lacunarity.ToString());
            thisElem.AddStringElement("Octaves", Octaves.ToString());
            thisElem.AddStringElement("Gain", Gain.ToString());
            thisElem.AddStringElement("PerturbFeatures", PerturbFeatures.ToString());
            thisElem.AddStringElement("Sharpness", Sharpness.ToString());
            thisElem.AddStringElement("AmplifyFeatures", AmplifyFeatures.ToString());
            thisElem.AddStringElement("AltitudeErosion", AltitudeErosion.ToString());
            thisElem.AddStringElement("RidgeErosion", RidgeErosion.ToString());
            thisElem.AddStringElement("SlopeErosion", SlopeErosion.ToString());
            thisElem.AddStringElement("Seed", Seed.ToString());
            thisElem.AddStringElement("Inverted", Inverted.ToString());
            thisElem.AddStringElement("Period", Period.ToTightString());
            thisElem.AddStringElement("Interpolation", Interpolation.ToString());
            thisElem.AddStringElement("Frequency", Frequency.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Lacunarity = thisElem.GetFloatElement("Lacunarity");
            Octaves = thisElem.GetIntElement("Octaves");
            Gain = thisElem.GetFloatElement("Gain");
            PerturbFeatures = thisElem.GetFloatElement("PerturbFeatures");
            Sharpness = thisElem.GetFloatElement("Sharpness");
            AmplifyFeatures = thisElem.GetFloatElement("AmplifyFeatures");
            AltitudeErosion = thisElem.GetFloatElement("AltitudeErosion");
            RidgeErosion = thisElem.GetFloatElement("RidgeErosion");
            SlopeErosion = thisElem.GetFloatElement("SlopeErosion");
            Seed = thisElem.GetIntElement("Seed");
            Inverted = thisElem.GetBoolElement("Inverted");
            Period = thisElem.GetStringElement("Period").ToVector2();
            Interpolation = (FastNoise.Interp)Enum.Parse(typeof(FastNoise.Interp), thisElem.GetStringElement("Interpolation"));
            Frequency = thisElem.GetFloatElement("Frequency");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Lacunarity);
            strm.Write(Octaves);
            strm.Write(Gain);
            strm.Write(PerturbFeatures);
            strm.Write(Sharpness);
            strm.Write(AmplifyFeatures);
            strm.Write(AltitudeErosion);
            strm.Write(RidgeErosion);
            strm.Write(SlopeErosion);
            strm.Write(Seed);
            strm.Write(Inverted);
            strm.Write(Period);
            strm.Write((int)Interpolation);
            strm.Write(Frequency);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Lacunarity = strm.ReadSingle();
            Octaves = strm.ReadInt32();
            Gain = strm.ReadSingle();
            PerturbFeatures = strm.ReadSingle();
            Sharpness = strm.ReadSingle();
            AmplifyFeatures = strm.ReadSingle();
            AltitudeErosion = strm.ReadSingle();
            RidgeErosion = strm.ReadSingle();
            SlopeErosion = strm.ReadSingle();
            Seed = strm.ReadInt32();
            Inverted = strm.ReadBoolean();
            Period = strm.ReadVector2();
            Interpolation = (FastNoise.Interp)strm.ReadInt32();
            Frequency = strm.ReadSingle();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            UberNoise retVal = new UberNoise();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            UberNoise retVal = into as UberNoise;
            retVal.lacunarity_ = this.lacunarity_;
            retVal.octaves_ = this.octaves_;
            retVal.gain_ = this.gain_;
            retVal.perturbFeatures_ = this.perturbFeatures_;
            retVal.sharpness_ = this.sharpness_;
            retVal.amplify_ = this.amplify_;
            retVal.altitudeErosion_ = this.altitudeErosion_;
            retVal.ridgeErosion_ = this.ridgeErosion_;
            retVal.slopeErosion_ = this.slopeErosion_;
            retVal.noise_ = this.noise_.Clone();
            retVal.inverted_ = this.inverted_;
            retVal.period_ = this.period_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class VoronoiGenerator
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Function", Function.ToString());
            thisElem.AddStringElement("CellType", CellType.ToString());
            thisElem.AddStringElement("KnockOutMode", KnockOutMode.ToString());
            thisElem.AddStringElement("Seed", Seed.ToString());
            thisElem.AddStringElement("Inverted", Inverted.ToString());
            thisElem.AddStringElement("Period", Period.ToTightString());
            thisElem.AddStringElement("Interpolation", Interpolation.ToString());
            thisElem.AddStringElement("Frequency", Frequency.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Function = (FastNoise.CellularDistanceFunction)Enum.Parse(typeof(FastNoise.CellularDistanceFunction), thisElem.GetStringElement("Function"));
            CellType = (FastNoise.CellularReturnType)Enum.Parse(typeof(FastNoise.CellularReturnType), thisElem.GetStringElement("CellType"));
            KnockOutMode = thisElem.GetBoolElement("KnockOutMode");
            Seed = thisElem.GetIntElement("Seed");
            Inverted = thisElem.GetBoolElement("Inverted");
            Period = thisElem.GetStringElement("Period").ToVector2();
            Interpolation = (FastNoise.Interp)Enum.Parse(typeof(FastNoise.Interp), thisElem.GetStringElement("Interpolation"));
            Frequency = thisElem.GetFloatElement("Frequency");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write((int)Function);
            strm.Write((int)CellType);
            strm.Write(KnockOutMode);
            strm.Write(Seed);
            strm.Write(Inverted);
            strm.Write(Period);
            strm.Write((int)Interpolation);
            strm.Write(Frequency);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Function = (FastNoise.CellularDistanceFunction)strm.ReadInt32();
            CellType = (FastNoise.CellularReturnType)strm.ReadInt32();
            KnockOutMode = strm.ReadBoolean();
            Seed = strm.ReadInt32();
            Inverted = strm.ReadBoolean();
            Period = strm.ReadVector2();
            Interpolation = (FastNoise.Interp)strm.ReadInt32();
            Frequency = strm.ReadSingle();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            VoronoiGenerator retVal = new VoronoiGenerator();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            VoronoiGenerator retVal = into as VoronoiGenerator;
            retVal.knockOutMode_ = this.knockOutMode_;
            retVal.noise_ = this.noise_.Clone();
            retVal.inverted_ = this.inverted_;
            retVal.period_ = this.period_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class WeaveGenerator
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("UnderRun", UnderRun.ToString());
            thisElem.AddStringElement("OverRun", OverRun.ToString());
            thisElem.AddStringElement("Skip", Skip.ToString());
            thisElem.AddStringElement("WarpWidth", WarpWidth.ToString());
            thisElem.AddStringElement("WeftWidth", WeftWidth.ToString());
            thisElem.AddStringElement("WarpColor", WarpColor.ToTightString());
            thisElem.AddStringElement("WeftColor", WeftColor.ToTightString());
            thisElem.AddStringElement("BaseColor", BaseColor.ToTightString());
            thisElem.AddStringElement("Tiling", Tiling.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            UnderRun = thisElem.GetIntElement("UnderRun");
            OverRun = thisElem.GetIntElement("OverRun");
            Skip = thisElem.GetIntElement("Skip");
            WarpWidth = thisElem.GetFloatElement("WarpWidth");
            WeftWidth = thisElem.GetFloatElement("WeftWidth");
            WarpColor = thisElem.GetStringElement("WarpColor").ToColor();
            WeftColor = thisElem.GetStringElement("WeftColor").ToColor();
            BaseColor = thisElem.GetStringElement("BaseColor").ToColor();
            Tiling = thisElem.GetStringElement("Tiling").ToIntVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(UnderRun);
            strm.Write(OverRun);
            strm.Write(Skip);
            strm.Write(WarpWidth);
            strm.Write(WeftWidth);
            strm.Write(WarpColor);
            strm.Write(WeftColor);
            strm.Write(BaseColor);
            strm.Write(Tiling);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            UnderRun = strm.ReadInt32();
            OverRun = strm.ReadInt32();
            Skip = strm.ReadInt32();
            WarpWidth = strm.ReadSingle();
            WeftWidth = strm.ReadSingle();
            WarpColor = strm.ReadColor();
            WeftColor = strm.ReadColor();
            BaseColor = strm.ReadColor();
            Tiling = strm.ReadIntVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            WeaveGenerator retVal = new WeaveGenerator();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            WeaveGenerator retVal = into as WeaveGenerator;
            retVal.underrun_ = this.underrun_;
            retVal.overrun_ = this.overrun_;
            retVal.skip_ = this.skip_;
            retVal.warpWidth_ = this.warpWidth_;
            retVal.weftWidth_ = this.weftWidth_;
            retVal.warpColor_ = this.warpColor_;
            retVal.weftColor_ = this.weftColor_;
            retVal.baseColor_ = this.baseColor_;
            retVal.tiling_ = this.tiling_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class WhiteNoiseGenerator
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Seed", Seed.ToString());
            thisElem.AddStringElement("Inverted", Inverted.ToString());
            thisElem.AddStringElement("Period", Period.ToTightString());
            thisElem.AddStringElement("Interpolation", Interpolation.ToString());
            thisElem.AddStringElement("Frequency", Frequency.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Seed = thisElem.GetIntElement("Seed");
            Inverted = thisElem.GetBoolElement("Inverted");
            Period = thisElem.GetStringElement("Period").ToVector2();
            Interpolation = (FastNoise.Interp)Enum.Parse(typeof(FastNoise.Interp), thisElem.GetStringElement("Interpolation"));
            Frequency = thisElem.GetFloatElement("Frequency");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Seed);
            strm.Write(Inverted);
            strm.Write(Period);
            strm.Write((int)Interpolation);
            strm.Write(Frequency);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Seed = strm.ReadInt32();
            Inverted = strm.ReadBoolean();
            Period = strm.ReadVector2();
            Interpolation = (FastNoise.Interp)strm.ReadInt32();
            Frequency = strm.ReadSingle();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            WhiteNoiseGenerator retVal = new WhiteNoiseGenerator();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            WhiteNoiseGenerator retVal = into as WhiteNoiseGenerator;
            retVal.noise_ = this.noise_.Clone();
            retVal.inverted_ = this.inverted_;
            retVal.period_ = this.period_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class AverageRGBNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            AverageRGBNode retVal = new AverageRGBNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            AverageRGBNode retVal = into as AverageRGBNode;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class BinarizeNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Cutoff", Cutoff.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Cutoff = thisElem.GetFloatElement("Cutoff");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Cutoff);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Cutoff = strm.ReadSingle();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            BinarizeNode retVal = new BinarizeNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            BinarizeNode retVal = into as BinarizeNode;
            retVal.cutoff_ = this.cutoff_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class ContrastNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Power", Power.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Power = thisElem.GetFloatElement("Power");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Power);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Power = strm.ReadSingle();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            ContrastNode retVal = new ContrastNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            ContrastNode retVal = into as ContrastNode;
            retVal.power_ = this.power_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class BrightnessNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Power", Power.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Power = thisElem.GetFloatElement("Power");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Power);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Power = strm.ReadSingle();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            BrightnessNode retVal = new BrightnessNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            BrightnessNode retVal = into as BrightnessNode;
            retVal.power_ = this.power_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class BrightnessRGBNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            BrightnessRGBNode retVal = new BrightnessRGBNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            BrightnessRGBNode retVal = into as BrightnessRGBNode;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class CurveTextureModifier
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Curves", Curves.ToTightString()); thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Curves = thisElem.GetStringElement("Curves").ToColorCurves(); Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Curves); strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Curves = strm.ReadColorCurves(); Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            CurveTextureModifier retVal = new CurveTextureModifier();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            CurveTextureModifier retVal = into as CurveTextureModifier;
            retVal.curves_ = this.curves_.Clone();
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class GradientRampTextureModifier
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Ramp", Ramp.ToTightString()); thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Ramp = thisElem.GetStringElement("Ramp").ToColorRamp(); Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Ramp); strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Ramp = strm.ReadColorRamp(); Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            GradientRampTextureModifier retVal = new GradientRampTextureModifier();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            GradientRampTextureModifier retVal = into as GradientRampTextureModifier;
            retVal.ramp_ = this.ramp_.Clone();
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class GradientLookupTable
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Index", Index.ToString());
            thisElem.AddStringElement("LUTHeight", LUTHeight.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Index = thisElem.GetIntElement("Index");
            LUTHeight = thisElem.GetIntElement("LUTHeight");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Index);
            strm.Write(LUTHeight);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Index = strm.ReadInt32();
            LUTHeight = strm.ReadInt32();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            GradientLookupTable retVal = new GradientLookupTable();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            GradientLookupTable retVal = into as GradientLookupTable;
            retVal.index_ = this.index_;
            retVal.lutHeight_ = this.lutHeight_;
            retVal.randomIndex_ = this.randomIndex_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class LevelsFilterNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("LowerBound", LowerBound.ToString());
            thisElem.AddStringElement("UpperBound", UpperBound.ToString());
            thisElem.AddStringElement("PivotFraction", PivotFraction.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            LowerBound = thisElem.GetFloatElement("LowerBound");
            UpperBound = thisElem.GetFloatElement("UpperBound");
            PivotFraction = thisElem.GetFloatElement("PivotFraction");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(LowerBound);
            strm.Write(UpperBound);
            strm.Write(PivotFraction);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            LowerBound = strm.ReadSingle();
            UpperBound = strm.ReadSingle();
            PivotFraction = strm.ReadSingle();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            LevelsFilterNode retVal = new LevelsFilterNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            LevelsFilterNode retVal = into as LevelsFilterNode;
            retVal.lowerBound_ = this.lowerBound_;
            retVal.pivotFraction_ = this.pivotFraction_;
            retVal.upperBound_ = this.upperBound_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class LookupTableRemapNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            LookupTableRemapNode retVal = new LookupTableRemapNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            LookupTableRemapNode retVal = into as LookupTableRemapNode;
            retVal.lutSize_ = this.lutSize_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class ReplaceColorModifier
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Replace", Replace.ToTightString());
            thisElem.AddStringElement("With", With.ToTightString());
            thisElem.AddStringElement("Tolerance", Tolerance.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Replace = thisElem.GetStringElement("Replace").ToColor();
            With = thisElem.GetStringElement("With").ToColor();
            Tolerance = thisElem.GetFloatElement("Tolerance");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Replace);
            strm.Write(With);
            strm.Write(Tolerance);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Replace = strm.ReadColor();
            With = strm.ReadColor();
            Tolerance = strm.ReadSingle();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            ReplaceColorModifier retVal = new ReplaceColorModifier();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            ReplaceColorModifier retVal = into as ReplaceColorModifier;
            retVal.replace_ = this.replace_;
            retVal.with_ = this.with_;
            retVal.tolerance_ = this.tolerance_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class SaturationFilterNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("LowerBound", LowerBound.ToString());
            thisElem.AddStringElement("UpperBound", UpperBound.ToString());
            thisElem.AddStringElement("PivotFraction", PivotFraction.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            LowerBound = thisElem.GetFloatElement("LowerBound");
            UpperBound = thisElem.GetFloatElement("UpperBound");
            PivotFraction = thisElem.GetFloatElement("PivotFraction");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(LowerBound);
            strm.Write(UpperBound);
            strm.Write(PivotFraction);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            LowerBound = strm.ReadSingle();
            UpperBound = strm.ReadSingle();
            PivotFraction = strm.ReadSingle();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            SaturationFilterNode retVal = new SaturationFilterNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            SaturationFilterNode retVal = into as SaturationFilterNode;
            retVal.lowerBound_ = this.lowerBound_;
            retVal.pivotFraction_ = this.pivotFraction_;
            retVal.upperBound_ = this.upperBound_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class SelectColorModifier
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Select", Select.ToTightString());
            thisElem.AddStringElement("Tolerance", Tolerance.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Select = thisElem.GetStringElement("Select").ToColor();
            Tolerance = thisElem.GetFloatElement("Tolerance");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Select);
            strm.Write(Tolerance);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Select = strm.ReadColor();
            Tolerance = strm.ReadSingle();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            SelectColorModifier retVal = new SelectColorModifier();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            SelectColorModifier retVal = into as SelectColorModifier;
            retVal.select_ = this.select_;
            retVal.tolerance_ = this.tolerance_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class FromGammaNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            FromGammaNode retVal = new FromGammaNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            FromGammaNode retVal = into as FromGammaNode;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class ToGammaNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            ToGammaNode retVal = new ToGammaNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            ToGammaNode retVal = into as ToGammaNode;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class CosNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            CosNode retVal = new CosNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            CosNode retVal = into as CosNode;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class ACosNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            ACosNode retVal = new ACosNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            ACosNode retVal = into as ACosNode;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class SinNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            SinNode retVal = new SinNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            SinNode retVal = into as SinNode;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class ASinNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            ASinNode retVal = new ASinNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            ASinNode retVal = into as ASinNode;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class TanNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            TanNode retVal = new TanNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            TanNode retVal = into as TanNode;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class ATanNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            ATanNode retVal = new ATanNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            ATanNode retVal = into as ATanNode;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class Clamp01Node
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            Clamp01Node retVal = new Clamp01Node();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            Clamp01Node retVal = into as Clamp01Node;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class ExpNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            ExpNode retVal = new ExpNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            ExpNode retVal = into as ExpNode;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class MaxRGBNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            MaxRGBNode retVal = new MaxRGBNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            MaxRGBNode retVal = into as MaxRGBNode;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class MinRGBNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            MinRGBNode retVal = new MinRGBNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            MinRGBNode retVal = into as MinRGBNode;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class PowNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            PowNode retVal = new PowNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            PowNode retVal = into as PowNode;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class SqrtNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            SqrtNode retVal = new SqrtNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            SqrtNode retVal = into as SqrtNode;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class FromNormalizedRangeNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Range", Range.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Range = thisElem.GetStringElement("Range").ToVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Range);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Range = strm.ReadVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            FromNormalizedRangeNode retVal = new FromNormalizedRangeNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            FromNormalizedRangeNode retVal = into as FromNormalizedRangeNode;
            retVal.range_ = this.range_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class ToNormalizedRangeNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Range", Range.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Range = thisElem.GetStringElement("Range").ToVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Range);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Range = strm.ReadVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            ToNormalizedRangeNode retVal = new ToNormalizedRangeNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            ToNormalizedRangeNode retVal = into as ToNormalizedRangeNode;
            retVal.range_ = this.range_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class AnisotropicBlur
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("EdgeCurve", EdgeCurve.ToString()); thisElem.AddStringElement("SobelEdgeStep", SobelEdgeStep.ToString());
            thisElem.AddStringElement("BlurRadius", BlurRadius.ToString());
            thisElem.AddStringElement("BlurStepSize", BlurStepSize.ToString());
            thisElem.AddStringElement("Sigma", Sigma.ToString());
            thisElem.AddStringElement("CacheScale", CacheScale.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            EdgeCurve = thisElem.GetStringElement("EdgeCurve").ToResponseCurve(); SobelEdgeStep = thisElem.GetFloatElement("SobelEdgeStep");
            BlurRadius = thisElem.GetIntElement("BlurRadius");
            BlurStepSize = thisElem.GetFloatElement("BlurStepSize");
            Sigma = thisElem.GetFloatElement("Sigma");
            CacheScale = thisElem.GetStringElement("CacheScale").ToVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(EdgeCurve); strm.Write(SobelEdgeStep);
            strm.Write(BlurRadius);
            strm.Write(BlurStepSize);
            strm.Write(Sigma);
            strm.Write(CacheScale);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            EdgeCurve = strm.ReadResponseCurve(); SobelEdgeStep = strm.ReadSingle();
            BlurRadius = strm.ReadInt32();
            BlurStepSize = strm.ReadSingle();
            Sigma = strm.ReadSingle();
            CacheScale = strm.ReadVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            AnisotropicBlur retVal = new AnisotropicBlur();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            AnisotropicBlur retVal = into as AnisotropicBlur;
            retVal.edgeCurve_ = this.edgeCurve_.Clone();
            retVal.sobelEdge_ = this.sobelEdge_;
            retVal.cacheScale_ = this.cacheScale_;
            retVal.lastExecutionContext = this.lastExecutionContext;
            retVal.cacheScale_ = this.cacheScale_;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class BlurModifier
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("BlurRadius", BlurRadius.ToString());
            thisElem.AddStringElement("BlurStepSize", BlurStepSize.ToString());
            thisElem.AddStringElement("Sigma", Sigma.ToString());
            thisElem.AddStringElement("CacheScale", CacheScale.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            BlurRadius = thisElem.GetIntElement("BlurRadius");
            BlurStepSize = thisElem.GetFloatElement("BlurStepSize");
            Sigma = thisElem.GetFloatElement("Sigma");
            CacheScale = thisElem.GetStringElement("CacheScale").ToVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(BlurRadius);
            strm.Write(BlurStepSize);
            strm.Write(Sigma);
            strm.Write(CacheScale);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            BlurRadius = strm.ReadInt32();
            BlurStepSize = strm.ReadSingle();
            Sigma = strm.ReadSingle();
            CacheScale = strm.ReadVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            BlurModifier retVal = new BlurModifier();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            BlurModifier retVal = into as BlurModifier;
            retVal.kernel = this.kernel;
            retVal.blurRadius_ = this.blurRadius_;
            retVal.blurStepSize_ = this.blurStepSize_;
            retVal.sigma_ = this.sigma_;
            retVal.cacheScale_ = this.cacheScale_;
            retVal.lastExecutionContext = this.lastExecutionContext;
            retVal.cacheScale_ = this.cacheScale_;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class ClipTextureModifier
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("ClipAlpha", ClipAlpha.ToString());
            thisElem.AddStringElement("Range", Range.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            ClipAlpha = thisElem.GetBoolElement("ClipAlpha");
            Range = thisElem.GetStringElement("Range").ToVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(ClipAlpha);
            strm.Write(Range);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            ClipAlpha = strm.ReadBoolean();
            Range = strm.ReadVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            ClipTextureModifier retVal = new ClipTextureModifier();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            ClipTextureModifier retVal = into as ClipTextureModifier;
            retVal.range_ = this.range_;
            retVal.clipAlpha_ = this.clipAlpha_;
            retVal.hasMin = this.hasMin;
            retVal.hasMax = this.hasMax;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class ConvolutionFilter
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Kernel", Kernel.ToTightString());
            thisElem.AddStringElement("StepSize", StepSize.ToString());
            thisElem.AddStringElement("CacheScale", CacheScale.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Kernel = thisElem.GetStringElement("Kernel").ToMat3x3();
            StepSize = thisElem.GetFloatElement("StepSize");
            CacheScale = thisElem.GetStringElement("CacheScale").ToVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Kernel);
            strm.Write(StepSize);
            strm.Write(CacheScale);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Kernel = strm.ReadMat3x3();
            StepSize = strm.ReadSingle();
            CacheScale = strm.ReadVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            ConvolutionFilter retVal = new ConvolutionFilter();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            ConvolutionFilter retVal = into as ConvolutionFilter;
            retVal.kernel_ = this.kernel_;
            retVal.stepSize_ = this.stepSize_;
            retVal.cacheScale_ = this.cacheScale_;
            retVal.lastExecutionContext = this.lastExecutionContext;
            retVal.cacheScale_ = this.cacheScale_;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class DivModifier
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Fraction", Fraction.ToString());
            thisElem.AddStringElement("Vertical", Vertical.ToString());
            thisElem.AddStringElement("NormalizeCoordinates", NormalizeCoordinates.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Fraction = thisElem.GetFloatElement("Fraction");
            Vertical = thisElem.GetBoolElement("Vertical");
            NormalizeCoordinates = thisElem.GetBoolElement("NormalizeCoordinates");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Fraction);
            strm.Write(Vertical);
            strm.Write(NormalizeCoordinates);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Fraction = strm.ReadSingle();
            Vertical = strm.ReadBoolean();
            NormalizeCoordinates = strm.ReadBoolean();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            DivModifier retVal = new DivModifier();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            DivModifier retVal = into as DivModifier;
            retVal.fraction_ = this.fraction_;
            retVal.vertical_ = this.vertical_;
            retVal.normalizeCoordinates_ = this.normalizeCoordinates_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class EmbossModifier
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Angle", Angle.ToString());
            thisElem.AddStringElement("StepSize", StepSize.ToString());
            thisElem.AddStringElement("Bias", Bias.ToString());
            thisElem.AddStringElement("Power", Power.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Angle = thisElem.GetFloatElement("Angle");
            StepSize = thisElem.GetFloatElement("StepSize");
            Bias = thisElem.GetFloatElement("Bias");
            Power = thisElem.GetFloatElement("Power");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Angle);
            strm.Write(StepSize);
            strm.Write(Bias);
            strm.Write(Power);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Angle = strm.ReadSingle();
            StepSize = strm.ReadSingle();
            Bias = strm.ReadSingle();
            Power = strm.ReadSingle();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            EmbossModifier retVal = new EmbossModifier();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            EmbossModifier retVal = into as EmbossModifier;
            retVal.angle_ = this.angle_;
            retVal.stepSize_ = this.stepSize_;
            retVal.bias_ = this.bias_;
            retVal.power_ = this.power_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class ErosionModifier
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Iterations", Iterations.ToString());
            thisElem.AddStringElement("Intensity", Intensity.ToString());
            thisElem.AddStringElement("StepSize", StepSize.ToString());
            thisElem.AddStringElement("Talus", Talus.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Iterations = thisElem.GetIntElement("Iterations");
            Intensity = thisElem.GetFloatElement("Intensity");
            StepSize = thisElem.GetFloatElement("StepSize");
            Talus = thisElem.GetFloatElement("Talus");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Iterations);
            strm.Write(Intensity);
            strm.Write(StepSize);
            strm.Write(Talus);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Iterations = strm.ReadInt32();
            Intensity = strm.ReadSingle();
            StepSize = strm.ReadSingle();
            Talus = strm.ReadSingle();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            ErosionModifier retVal = new ErosionModifier();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            ErosionModifier retVal = into as ErosionModifier;
            retVal.iterations_ = this.iterations_;
            retVal.intensity_ = this.intensity_;
            retVal.stepSize_ = this.stepSize_;
            retVal.talus_ = this.talus_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class FillEmpty
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            FillEmpty retVal = new FillEmpty();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            FillEmpty retVal = into as FillEmpty;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class InvertTextureModifier
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            InvertTextureModifier retVal = new InvertTextureModifier();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            InvertTextureModifier retVal = into as InvertTextureModifier;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class OctaveSum
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Lacunarity", Lacunarity.ToTightString());
            thisElem.AddStringElement("Octaves", Octaves.ToString());
            thisElem.AddStringElement("Gain", Gain.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Lacunarity = thisElem.GetStringElement("Lacunarity").ToVector2();
            Octaves = thisElem.GetIntElement("Octaves");
            Gain = thisElem.GetFloatElement("Gain");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Lacunarity);
            strm.Write(Octaves);
            strm.Write(Gain);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Lacunarity = strm.ReadVector2();
            Octaves = strm.ReadInt32();
            Gain = strm.ReadSingle();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            OctaveSum retVal = new OctaveSum();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            OctaveSum retVal = into as OctaveSum;
            retVal.lacunarity_ = this.lacunarity_;
            retVal.octaves_ = this.octaves_;
            retVal.gain_ = this.gain_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class HatchFilter
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("Power", Power.ToString());
            thisElem.AddStringElement("StepSize", StepSize.ToString());
            thisElem.AddStringElement("Pinch", Pinch.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Description = thisElem.GetStringElement("Description");
            Power = thisElem.GetFloatElement("Power");
            StepSize = thisElem.GetFloatElement("StepSize");
            Pinch = (SprueKit.Data.TexGen.HatchFilter.PinchMode)Enum.Parse(typeof(SprueKit.Data.TexGen.HatchFilter.PinchMode), thisElem.GetStringElement("Pinch"));
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Description ?? "");
            strm.Write(Power);
            strm.Write(StepSize);
            strm.Write((int)Pinch);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Description = strm.ReadString();
            Power = strm.ReadSingle();
            StepSize = strm.ReadSingle();
            Pinch = (SprueKit.Data.TexGen.HatchFilter.PinchMode)strm.ReadInt32();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            HatchFilter retVal = new HatchFilter();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            HatchFilter retVal = into as HatchFilter;
            retVal.power_ = this.power_;
            retVal.pinch_ = this.pinch_;
            retVal.stepSize_ = this.stepSize_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class PosterizeModifier
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Range", Range.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Range = thisElem.GetIntElement("Range");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Range);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Range = strm.ReadInt32();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            PosterizeModifier retVal = new PosterizeModifier();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            PosterizeModifier retVal = into as PosterizeModifier;
            retVal.range_ = this.range_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class SharpenFilter
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Power", Power.ToString());
            thisElem.AddStringElement("StepSize", StepSize.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Power = thisElem.GetFloatElement("Power");
            StepSize = thisElem.GetFloatElement("StepSize");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Power);
            strm.Write(StepSize);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Power = strm.ReadSingle();
            StepSize = strm.ReadSingle();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            SharpenFilter retVal = new SharpenFilter();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            SharpenFilter retVal = into as SharpenFilter;
            retVal.power_ = this.power_;
            retVal.stepSize_ = this.stepSize_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class SobelTextureModifier
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("StepSize", StepSize.ToString());
            thisElem.AddStringElement("CacheScale", CacheScale.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            StepSize = thisElem.GetFloatElement("StepSize");
            CacheScale = thisElem.GetStringElement("CacheScale").ToVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(StepSize);
            strm.Write(CacheScale);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            StepSize = strm.ReadSingle();
            CacheScale = strm.ReadVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            SobelTextureModifier retVal = new SobelTextureModifier();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            SobelTextureModifier retVal = into as SobelTextureModifier;
            retVal.stepSize_ = this.stepSize_;
            retVal.cacheScale_ = this.cacheScale_;
            retVal.lastExecutionContext = this.lastExecutionContext;
            retVal.cacheScale_ = this.cacheScale_;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class SolarizeTextureModifier
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Threshold", Threshold.ToString());
            thisElem.AddStringElement("InvertLower", InvertLower.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Threshold = thisElem.GetFloatElement("Threshold");
            InvertLower = thisElem.GetBoolElement("InvertLower");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Threshold);
            strm.Write(InvertLower);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Threshold = strm.ReadSingle();
            InvertLower = strm.ReadBoolean();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            SolarizeTextureModifier retVal = new SolarizeTextureModifier();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            SolarizeTextureModifier retVal = into as SolarizeTextureModifier;
            retVal.threshold_ = this.threshold_;
            retVal.invertLower_ = this.invertLower_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class StreakModifier
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("StreakAngle", StreakAngle.ToString());
            thisElem.AddStringElement("StreakLength", StreakLength.ToString());
            thisElem.AddStringElement("Samples", Samples.ToString());
            thisElem.AddStringElement("CacheScale", CacheScale.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            StreakAngle = thisElem.GetFloatElement("StreakAngle");
            StreakLength = thisElem.GetFloatElement("StreakLength");
            Samples = thisElem.GetIntElement("Samples");
            CacheScale = thisElem.GetStringElement("CacheScale").ToVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(StreakAngle);
            strm.Write(StreakLength);
            strm.Write(Samples);
            strm.Write(CacheScale);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            StreakAngle = strm.ReadSingle();
            StreakLength = strm.ReadSingle();
            Samples = strm.ReadInt32();
            CacheScale = strm.ReadVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            StreakModifier retVal = new StreakModifier();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            StreakModifier retVal = into as StreakModifier;
            retVal.streakAngle_ = this.streakAngle_;
            retVal.streakLength_ = this.streakLength_;
            retVal.samples_ = this.samples_;
            retVal.fadeOff_ = this.fadeOff_;
            retVal.cacheScale_ = this.cacheScale_;
            retVal.lastExecutionContext = this.lastExecutionContext;
            retVal.cacheScale_ = this.cacheScale_;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class TransformModifier
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Transform", Transform.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Transform = thisElem.GetStringElement("Transform").ToMat3x3();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Transform);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Transform = strm.ReadMat3x3();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            TransformModifier retVal = new TransformModifier();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            TransformModifier retVal = into as TransformModifier;
            retVal.transform_ = this.transform_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class SimpleTransformModifier
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Offset", Offset.ToTightString());
            thisElem.AddStringElement("Scale", Scale.ToTightString());
            thisElem.AddStringElement("Rotation", Rotation.ToString());
            thisElem.AddStringElement("ClipBounds", ClipBounds.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Offset = thisElem.GetStringElement("Offset").ToVector2();
            Scale = thisElem.GetStringElement("Scale").ToVector2();
            Rotation = thisElem.GetFloatElement("Rotation");
            ClipBounds = thisElem.GetBoolElement("ClipBounds");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Offset);
            strm.Write(Scale);
            strm.Write(Rotation);
            strm.Write(ClipBounds);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Offset = strm.ReadVector2();
            Scale = strm.ReadVector2();
            Rotation = strm.ReadSingle();
            ClipBounds = strm.ReadBoolean();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            SimpleTransformModifier retVal = new SimpleTransformModifier();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            SimpleTransformModifier retVal = into as SimpleTransformModifier;
            retVal.offset_ = this.offset_;
            retVal.rotation_ = this.rotation_;
            retVal.scale_ = this.scale_;
            retVal.clipBounds_ = this.clipBounds_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class SplatMapNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            SplatMapNode retVal = new SplatMapNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            SplatMapNode retVal = into as SplatMapNode;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class TrimModifier
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("LeftTrimSize", LeftTrimSize.ToString());
            thisElem.AddStringElement("RightTrimSize", RightTrimSize.ToString());
            thisElem.AddStringElement("Vertical", Vertical.ToString());
            thisElem.AddStringElement("NormalizeCoordinates", NormalizeCoordinates.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            LeftTrimSize = thisElem.GetFloatElement("LeftTrimSize");
            RightTrimSize = thisElem.GetFloatElement("RightTrimSize");
            Vertical = thisElem.GetBoolElement("Vertical");
            NormalizeCoordinates = thisElem.GetBoolElement("NormalizeCoordinates");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(LeftTrimSize);
            strm.Write(RightTrimSize);
            strm.Write(Vertical);
            strm.Write(NormalizeCoordinates);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            LeftTrimSize = strm.ReadSingle();
            RightTrimSize = strm.ReadSingle();
            Vertical = strm.ReadBoolean();
            NormalizeCoordinates = strm.ReadBoolean();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            TrimModifier retVal = new TrimModifier();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            TrimModifier retVal = into as TrimModifier;
            retVal.leftTrimSize_ = this.leftTrimSize_;
            retVal.rightTrimSize_ = this.rightTrimSize_;
            retVal.vertical_ = this.vertical_;
            retVal.normalizeCoordinates_ = this.normalizeCoordinates_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class TileModifier
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Tiling", Tiling.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Tiling = thisElem.GetStringElement("Tiling").ToVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Tiling);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Tiling = strm.ReadVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            TileModifier retVal = new TileModifier();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            TileModifier retVal = into as TileModifier;
            retVal.tiling_ = this.tiling_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class WarpModifier
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Intensity", Intensity.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Intensity = thisElem.GetStringElement("Intensity").ToVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Intensity);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Intensity = strm.ReadVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            WarpModifier retVal = new WarpModifier();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            WarpModifier retVal = into as WarpModifier;
            retVal.intensity_ = this.intensity_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class WaveFilter
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Angle", Angle.ToString());
            thisElem.AddStringElement("Amplitude", Amplitude.ToString());
            thisElem.AddStringElement("Frequency", Frequency.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Angle = thisElem.GetFloatElement("Angle");
            Amplitude = thisElem.GetFloatElement("Amplitude");
            Frequency = thisElem.GetFloatElement("Frequency");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Angle);
            strm.Write(Amplitude);
            strm.Write(Frequency);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Angle = strm.ReadSingle();
            Amplitude = strm.ReadSingle();
            Frequency = strm.ReadSingle();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            WaveFilter retVal = new WaveFilter();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            WaveFilter retVal = into as WaveFilter;
            retVal.angle_ = this.angle_;
            retVal.frequency_ = this.frequency_;
            retVal.amplitude_ = this.amplitude_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class CartesianToPolarModifier
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            CartesianToPolarModifier retVal = new CartesianToPolarModifier();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            CartesianToPolarModifier retVal = into as CartesianToPolarModifier;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class PolarToCartesianModifier
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            PolarToCartesianModifier retVal = new PolarToCartesianModifier();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            PolarToCartesianModifier retVal = into as PolarToCartesianModifier;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class AmbientOcclusionBaker
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("TextureSize", TextureSize.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            TextureSize = thisElem.GetStringElement("TextureSize").ToIntVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(TextureSize);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            TextureSize = strm.ReadIntVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            AmbientOcclusionBaker retVal = new AmbientOcclusionBaker();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            AmbientOcclusionBaker retVal = into as AmbientOcclusionBaker;
            retVal.cache_ = this.cache_;
            retVal.dirty_ = this.dirty_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class CurvatureBaker
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("TextureSize", TextureSize.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            TextureSize = thisElem.GetStringElement("TextureSize").ToIntVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(TextureSize);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            TextureSize = strm.ReadIntVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            CurvatureBaker retVal = new CurvatureBaker();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            CurvatureBaker retVal = into as CurvatureBaker;
            retVal.cache_ = this.cache_;
            retVal.dirty_ = this.dirty_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class DominantPlaneBaker
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("TextureSize", TextureSize.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            TextureSize = thisElem.GetStringElement("TextureSize").ToIntVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(TextureSize);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            TextureSize = strm.ReadIntVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            DominantPlaneBaker retVal = new DominantPlaneBaker();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            DominantPlaneBaker retVal = into as DominantPlaneBaker;
            retVal.cache_ = this.cache_;
            retVal.dirty_ = this.dirty_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class FacetBaker
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("AngularTolerance", AngularTolerance.ToString());
            thisElem.AddStringElement("DrawAllEdges", DrawAllEdges.ToString());
            thisElem.AddStringElement("TextureSize", TextureSize.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            AngularTolerance = thisElem.GetFloatElement("AngularTolerance");
            DrawAllEdges = thisElem.GetBoolElement("DrawAllEdges");
            TextureSize = thisElem.GetStringElement("TextureSize").ToIntVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(AngularTolerance);
            strm.Write(DrawAllEdges);
            strm.Write(TextureSize);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            AngularTolerance = strm.ReadSingle();
            DrawAllEdges = strm.ReadBoolean();
            TextureSize = strm.ReadIntVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            FacetBaker retVal = new FacetBaker();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            FacetBaker retVal = into as FacetBaker;
            retVal.angularTolerance_ = this.angularTolerance_;
            retVal.forceAllEdges_ = this.forceAllEdges_;
            retVal.cache_ = this.cache_;
            retVal.dirty_ = this.dirty_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class FauxLightBaker
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("FlipNormals", FlipNormals.ToString());
            thisElem.AddStringElement("LCSMRelax", LCSMRelax.ToString());
            thisElem.AddStringElement("LightColor", LightColor.ToTightString());
            thisElem.AddStringElement("LightDirection", LightDirection.ToTightString());
            if (Texture != null && System.IO.File.Exists(Texture.AbsolutePath))
                thisElem.AddStringElement("Texture", ctx.GetRelativePathString(Texture));
            thisElem.AddStringElement("Tiling", Tiling.ToTightString());
            thisElem.AddStringElement("TextureSize", TextureSize.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            FlipNormals = thisElem.GetBoolElement("FlipNormals");
            LCSMRelax = thisElem.GetBoolElement("LCSMRelax");
            LightColor = thisElem.GetStringElement("LightColor").ToColor();
            LightDirection = thisElem.GetStringElement("LightDirection").ToVector3();
            {
                string fileString = thisElem.GetStringElement("Texture");
                if (!string.IsNullOrWhiteSpace(fileString))
                {
                    Uri result = ctx.GetAbsolutePath(new Uri(thisElem.GetStringElement(fileString)), this, "FauxLightBaker", "FauxLightBaker", FileData.ImageFileMask);
                    if (result != null) Texture = result;
                }
            }
            Tiling = thisElem.GetStringElement("Tiling").ToVector2();
            TextureSize = thisElem.GetStringElement("TextureSize").ToIntVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(FlipNormals);
            strm.Write(LCSMRelax);
            strm.Write(LightColor);
            strm.Write(LightDirection);
            if (Texture != null && System.IO.File.Exists(Texture.AbsolutePath))
            {
                strm.Write(true);
                strm.Write(ctx.GetRelativePathString(Texture));
            }
            else strm.Write(false);
            strm.Write(Tiling);
            strm.Write(TextureSize);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            FlipNormals = strm.ReadBoolean();
            LCSMRelax = strm.ReadBoolean();
            LightColor = strm.ReadColor();
            LightDirection = strm.ReadVector3();
            if (strm.ReadBoolean())
            {
                Uri result = ctx.GetAbsolutePath(new Uri(strm.ReadString()), this, "FauxLightBaker", "FauxLightBaker", FileData.ImageFileMask);
                if (result != null) Texture = result;
            }
            Tiling = strm.ReadVector2();
            TextureSize = strm.ReadIntVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            FauxLightBaker retVal = new FauxLightBaker();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            FauxLightBaker retVal = into as FauxLightBaker;
            retVal.flipNormals_ = this.flipNormals_;
            retVal.blur_ = this.blur_;
            retVal.lightColor_ = this.lightColor_;
            retVal.lightDirection_ = this.lightDirection_;
            retVal.data_ = this.data_;
            retVal.imageFile_ = this.imageFile_;
            retVal.bilinearFilter_ = this.bilinearFilter_;
            retVal.cache_ = this.cache_;
            retVal.dirty_ = this.dirty_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class ModelSpaceGradientBaker
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("TextureSize", TextureSize.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            TextureSize = thisElem.GetStringElement("TextureSize").ToIntVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(TextureSize);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            TextureSize = strm.ReadIntVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            ModelSpaceGradientBaker retVal = new ModelSpaceGradientBaker();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            ModelSpaceGradientBaker retVal = into as ModelSpaceGradientBaker;
            retVal.cache_ = this.cache_;
            retVal.dirty_ = this.dirty_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class ModelSpaceNormalBaker
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("TextureSize", TextureSize.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            TextureSize = thisElem.GetStringElement("TextureSize").ToIntVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(TextureSize);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            TextureSize = strm.ReadIntVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            ModelSpaceNormalBaker retVal = new ModelSpaceNormalBaker();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            ModelSpaceNormalBaker retVal = into as ModelSpaceNormalBaker;
            retVal.cache_ = this.cache_;
            retVal.dirty_ = this.dirty_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class ModelSpacePositionBaker
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("TextureSize", TextureSize.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            TextureSize = thisElem.GetStringElement("TextureSize").ToIntVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(TextureSize);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            TextureSize = strm.ReadIntVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            ModelSpacePositionBaker retVal = new ModelSpacePositionBaker();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            ModelSpacePositionBaker retVal = into as ModelSpacePositionBaker;
            retVal.cache_ = this.cache_;
            retVal.dirty_ = this.dirty_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class ThicknessBaker
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("TextureSize", TextureSize.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            TextureSize = thisElem.GetStringElement("TextureSize").ToIntVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(TextureSize);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            TextureSize = strm.ReadIntVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            ThicknessBaker retVal = new ThicknessBaker();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            ThicknessBaker retVal = into as ThicknessBaker;
            retVal.cache_ = this.cache_;
            retVal.dirty_ = this.dirty_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class TriplanarTextureBaker
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("TriplanarSampling", TriplanarSampling.ToString());
            if (Texture != null && System.IO.File.Exists(Texture.AbsolutePath))
                thisElem.AddStringElement("Texture", ctx.GetRelativePathString(Texture));
            thisElem.AddStringElement("Tiling", Tiling.ToTightString());
            thisElem.AddStringElement("TextureSize", TextureSize.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            TriplanarSampling = thisElem.GetBoolElement("TriplanarSampling");
            {
                string fileString = thisElem.GetStringElement("Texture");
                if (!string.IsNullOrWhiteSpace(fileString))
                {
                    Uri result = ctx.GetAbsolutePath(new Uri(thisElem.GetStringElement(fileString)), this, "TriplanarTextureBaker", "TriplanarTextureBaker", FileData.ImageFileMask);
                    if (result != null) Texture = result;
                }
            }
            Tiling = thisElem.GetStringElement("Tiling").ToVector2();
            TextureSize = thisElem.GetStringElement("TextureSize").ToIntVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(TriplanarSampling);
            if (Texture != null && System.IO.File.Exists(Texture.AbsolutePath))
            {
                strm.Write(true);
                strm.Write(ctx.GetRelativePathString(Texture));
            }
            else strm.Write(false);
            strm.Write(Tiling);
            strm.Write(TextureSize);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            TriplanarSampling = strm.ReadBoolean();
            if (strm.ReadBoolean())
            {
                Uri result = ctx.GetAbsolutePath(new Uri(strm.ReadString()), this, "TriplanarTextureBaker", "TriplanarTextureBaker", FileData.ImageFileMask);
                if (result != null) Texture = result;
            }
            Tiling = strm.ReadVector2();
            TextureSize = strm.ReadIntVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            TriplanarTextureBaker retVal = new TriplanarTextureBaker();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            TriplanarTextureBaker retVal = into as TriplanarTextureBaker;
            retVal.useTriplanar_ = this.useTriplanar_;
            retVal.data_ = this.data_;
            retVal.imageFile_ = this.imageFile_;
            retVal.bilinearFilter_ = this.bilinearFilter_;
            retVal.cache_ = this.cache_;
            retVal.dirty_ = this.dirty_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class CylindricalTextureBaker
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            if (Texture != null && System.IO.File.Exists(Texture.AbsolutePath))
                thisElem.AddStringElement("Texture", ctx.GetRelativePathString(Texture));
            thisElem.AddStringElement("Tiling", Tiling.ToTightString());
            thisElem.AddStringElement("TextureSize", TextureSize.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            {
                string fileString = thisElem.GetStringElement("Texture");
                if (!string.IsNullOrWhiteSpace(fileString))
                {
                    Uri result = ctx.GetAbsolutePath(new Uri(thisElem.GetStringElement(fileString)), this, "CylindricalTextureBaker", "CylindricalTextureBaker", FileData.ImageFileMask);
                    if (result != null) Texture = result;
                }
            }
            Tiling = thisElem.GetStringElement("Tiling").ToVector2();
            TextureSize = thisElem.GetStringElement("TextureSize").ToIntVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            if (Texture != null && System.IO.File.Exists(Texture.AbsolutePath))
            {
                strm.Write(true);
                strm.Write(ctx.GetRelativePathString(Texture));
            }
            else strm.Write(false);
            strm.Write(Tiling);
            strm.Write(TextureSize);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            if (strm.ReadBoolean())
            {
                Uri result = ctx.GetAbsolutePath(new Uri(strm.ReadString()), this, "CylindricalTextureBaker", "CylindricalTextureBaker", FileData.ImageFileMask);
                if (result != null) Texture = result;
            }
            Tiling = strm.ReadVector2();
            TextureSize = strm.ReadIntVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            CylindricalTextureBaker retVal = new CylindricalTextureBaker();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            CylindricalTextureBaker retVal = into as CylindricalTextureBaker;
            retVal.data_ = this.data_;
            retVal.imageFile_ = this.imageFile_;
            retVal.bilinearFilter_ = this.bilinearFilter_;
            retVal.cache_ = this.cache_;
            retVal.dirty_ = this.dirty_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class VolumetricFBM
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Fractal", Fractal.ToString());
            thisElem.AddStringElement("Gain", Gain.ToString());
            thisElem.AddStringElement("Lacunarity", Lacunarity.ToString());
            thisElem.AddStringElement("Octaves", Octaves.ToString());
            thisElem.AddStringElement("Seed", Seed.ToString());
            thisElem.AddStringElement("Inverted", Inverted.ToString());
            thisElem.AddStringElement("Interpolation", Interpolation.ToString());
            thisElem.AddStringElement("Frequency", Frequency.ToString());
            thisElem.AddStringElement("TextureSize", TextureSize.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Fractal = (FastNoise.FractalType)Enum.Parse(typeof(FastNoise.FractalType), thisElem.GetStringElement("Fractal"));
            Gain = thisElem.GetFloatElement("Gain");
            Lacunarity = thisElem.GetFloatElement("Lacunarity");
            Octaves = thisElem.GetIntElement("Octaves");
            Seed = thisElem.GetIntElement("Seed");
            Inverted = thisElem.GetBoolElement("Inverted");
            Interpolation = (FastNoise.Interp)Enum.Parse(typeof(FastNoise.Interp), thisElem.GetStringElement("Interpolation"));
            Frequency = thisElem.GetFloatElement("Frequency");
            TextureSize = thisElem.GetStringElement("TextureSize").ToIntVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write((int)Fractal);
            strm.Write(Gain);
            strm.Write(Lacunarity);
            strm.Write(Octaves);
            strm.Write(Seed);
            strm.Write(Inverted);
            strm.Write((int)Interpolation);
            strm.Write(Frequency);
            strm.Write(TextureSize);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Fractal = (FastNoise.FractalType)strm.ReadInt32();
            Gain = strm.ReadSingle();
            Lacunarity = strm.ReadSingle();
            Octaves = strm.ReadInt32();
            Seed = strm.ReadInt32();
            Inverted = strm.ReadBoolean();
            Interpolation = (FastNoise.Interp)strm.ReadInt32();
            Frequency = strm.ReadSingle();
            TextureSize = strm.ReadIntVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            VolumetricFBM retVal = new VolumetricFBM();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            VolumetricFBM retVal = into as VolumetricFBM;
            retVal.noise_ = this.noise_.Clone();
            retVal.inverted_ = this.inverted_;
            retVal.period_ = this.period_;
            retVal.cache_ = this.cache_;
            retVal.dirty_ = this.dirty_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class VolumetricPerlin
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Seed", Seed.ToString());
            thisElem.AddStringElement("Inverted", Inverted.ToString());
            thisElem.AddStringElement("Interpolation", Interpolation.ToString());
            thisElem.AddStringElement("Frequency", Frequency.ToString());
            thisElem.AddStringElement("TextureSize", TextureSize.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Seed = thisElem.GetIntElement("Seed");
            Inverted = thisElem.GetBoolElement("Inverted");
            Interpolation = (FastNoise.Interp)Enum.Parse(typeof(FastNoise.Interp), thisElem.GetStringElement("Interpolation"));
            Frequency = thisElem.GetFloatElement("Frequency");
            TextureSize = thisElem.GetStringElement("TextureSize").ToIntVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Seed);
            strm.Write(Inverted);
            strm.Write((int)Interpolation);
            strm.Write(Frequency);
            strm.Write(TextureSize);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Seed = strm.ReadInt32();
            Inverted = strm.ReadBoolean();
            Interpolation = (FastNoise.Interp)strm.ReadInt32();
            Frequency = strm.ReadSingle();
            TextureSize = strm.ReadIntVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            VolumetricPerlin retVal = new VolumetricPerlin();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            VolumetricPerlin retVal = into as VolumetricPerlin;
            retVal.noise_ = this.noise_.Clone();
            retVal.inverted_ = this.inverted_;
            retVal.period_ = this.period_;
            retVal.cache_ = this.cache_;
            retVal.dirty_ = this.dirty_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class VolumetricVoronoi
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Function", Function.ToString());
            thisElem.AddStringElement("CellType", CellType.ToString());
            thisElem.AddStringElement("KnockOutMode", KnockOutMode.ToString());
            thisElem.AddStringElement("Seed", Seed.ToString());
            thisElem.AddStringElement("Inverted", Inverted.ToString());
            thisElem.AddStringElement("Interpolation", Interpolation.ToString());
            thisElem.AddStringElement("Frequency", Frequency.ToString());
            thisElem.AddStringElement("TextureSize", TextureSize.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Function = (FastNoise.CellularDistanceFunction)Enum.Parse(typeof(FastNoise.CellularDistanceFunction), thisElem.GetStringElement("Function"));
            CellType = (FastNoise.CellularReturnType)Enum.Parse(typeof(FastNoise.CellularReturnType), thisElem.GetStringElement("CellType"));
            KnockOutMode = thisElem.GetBoolElement("KnockOutMode");
            Seed = thisElem.GetIntElement("Seed");
            Inverted = thisElem.GetBoolElement("Inverted");
            Interpolation = (FastNoise.Interp)Enum.Parse(typeof(FastNoise.Interp), thisElem.GetStringElement("Interpolation"));
            Frequency = thisElem.GetFloatElement("Frequency");
            TextureSize = thisElem.GetStringElement("TextureSize").ToIntVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write((int)Function);
            strm.Write((int)CellType);
            strm.Write(KnockOutMode);
            strm.Write(Seed);
            strm.Write(Inverted);
            strm.Write((int)Interpolation);
            strm.Write(Frequency);
            strm.Write(TextureSize);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Function = (FastNoise.CellularDistanceFunction)strm.ReadInt32();
            CellType = (FastNoise.CellularReturnType)strm.ReadInt32();
            KnockOutMode = strm.ReadBoolean();
            Seed = strm.ReadInt32();
            Inverted = strm.ReadBoolean();
            Interpolation = (FastNoise.Interp)strm.ReadInt32();
            Frequency = strm.ReadSingle();
            TextureSize = strm.ReadIntVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            VolumetricVoronoi retVal = new VolumetricVoronoi();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            VolumetricVoronoi retVal = into as VolumetricVoronoi;
            retVal.knockOutMode_ = this.knockOutMode_;
            retVal.noise_ = this.noise_.Clone();
            retVal.inverted_ = this.inverted_;
            retVal.period_ = this.period_;
            retVal.cache_ = this.cache_;
            retVal.dirty_ = this.dirty_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class SimpleReliefNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("RenderSize", RenderSize.ToTightString());
            thisElem.AddStringElement("Angle", Angle.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            RenderSize = thisElem.GetStringElement("RenderSize").ToIntVector2();
            Angle = (SprueKit.Data.TexGen.BakeViewAngle)Enum.Parse(typeof(SprueKit.Data.TexGen.BakeViewAngle), thisElem.GetStringElement("Angle"));
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(RenderSize);
            strm.Write((int)Angle);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            RenderSize = strm.ReadIntVector2();
            Angle = (SprueKit.Data.TexGen.BakeViewAngle)strm.ReadInt32();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            SimpleReliefNode retVal = new SimpleReliefNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            SimpleReliefNode retVal = into as SimpleReliefNode;
            retVal.bakeSize_ = this.bakeSize_;
            retVal.angle_ = this.angle_;
            retVal.dataCache_ = this.dataCache_;
            retVal.cache_ = this.cache_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class NormalMapDeviation
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            NormalMapDeviation retVal = new NormalMapDeviation();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            NormalMapDeviation retVal = into as NormalMapDeviation;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class NormalMapNormalize
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            NormalMapNormalize retVal = new NormalMapNormalize();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            NormalMapNormalize retVal = into as NormalMapNormalize;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class NormalPower
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Power", Power.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Power = thisElem.GetFloatElement("Power");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Power);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Power = strm.ReadSingle();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            NormalPower retVal = new NormalPower();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            NormalPower retVal = into as NormalPower;
            retVal.power_ = this.power_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class ToNormalMap
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("StepSize", StepSize.ToString());
            thisElem.AddStringElement("Power", Power.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            StepSize = thisElem.GetFloatElement("StepSize");
            Power = thisElem.GetFloatElement("Power");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(StepSize);
            strm.Write(Power);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            StepSize = strm.ReadSingle();
            Power = strm.ReadSingle();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            ToNormalMap retVal = new ToNormalMap();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            ToNormalMap retVal = into as ToNormalMap;
            retVal.stepSize_ = this.stepSize_;
            retVal.power_ = this.power_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class RotateNormals
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Rotation", Rotation.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Rotation = thisElem.GetStringElement("Rotation").ToVector3();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Rotation);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Rotation = strm.ReadVector3();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            RotateNormals retVal = new RotateNormals();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            RotateNormals retVal = into as RotateNormals;
            retVal.rotation_ = this.rotation_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class WarpOut
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("WarpKey", WarpKey);
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            WarpKey = thisElem.GetStringElement("WarpKey");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(WarpKey ?? "");
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            WarpKey = strm.ReadString();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            WarpOut retVal = new WarpOut();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            WarpOut retVal = into as WarpOut;
            retVal.warpKey_ = this.warpKey_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class WarpIn
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("WarpKey", WarpKey);
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            WarpKey = thisElem.GetStringElement("WarpKey");
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(WarpKey ?? "");
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            WarpKey = strm.ReadString();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            WarpIn retVal = new WarpIn();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            WarpIn retVal = into as WarpIn;
            retVal.warp_ = this.warp_;
            retVal.warpKey_ = this.warpKey_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class SampleControl
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("CacheSize", CacheSize.ToTightString());
            thisElem.AddStringElement("CacheScale", CacheScale.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            CacheSize = thisElem.GetStringElement("CacheSize").ToIntVector2();
            CacheScale = thisElem.GetStringElement("CacheScale").ToVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(CacheSize);
            strm.Write(CacheScale);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            CacheSize = strm.ReadIntVector2();
            CacheScale = strm.ReadVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            SampleControl retVal = new SampleControl();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            SampleControl retVal = into as SampleControl;
            retVal.cacheSize_ = this.cacheSize_;
            retVal.cacheScale_ = this.cacheScale_;
            retVal.lastExecutionContext = this.lastExecutionContext;
            retVal.cacheScale_ = this.cacheScale_;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class TextureOutputNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("OutputChannel", OutputChannel.ToString());
            thisElem.AddStringElement("DefaultColor", DefaultColor.ToTightString());
            thisElem.AddStringElement("PreviewSize", PreviewSize.ToTightString());
            thisElem.AddStringElement("TargetSize", TargetSize.ToTightString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            OutputChannel = (SprueKit.Data.TextureChannel)Enum.Parse(typeof(SprueKit.Data.TextureChannel), thisElem.GetStringElement("OutputChannel"));
            DefaultColor = thisElem.GetStringElement("DefaultColor").ToColor();
            PreviewSize = thisElem.GetStringElement("PreviewSize").ToIntVector2();
            TargetSize = thisElem.GetStringElement("TargetSize").ToIntVector2();
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write((int)OutputChannel);
            strm.Write(DefaultColor);
            strm.Write(PreviewSize);
            strm.Write(TargetSize);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            OutputChannel = (SprueKit.Data.TextureChannel)strm.ReadInt32();
            DefaultColor = strm.ReadColor();
            PreviewSize = strm.ReadIntVector2();
            TargetSize = strm.ReadIntVector2();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            TextureOutputNode retVal = new TextureOutputNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            TextureOutputNode retVal = into as TextureOutputNode;
            retVal.outputChannel_ = this.outputChannel_;
            retVal.defaultColor_ = this.defaultColor_;
            retVal.targetSize_ = this.targetSize_;
            retVal.exportSize_ = this.exportSize_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
namespace SprueKit.Data.TexGen
{
    public partial class BlendNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("BlendMode", BlendMode.ToString());
            thisElem.AddStringElement("BlendSource", BlendSource.ToString());
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            BlendMode = (SprueKit.Data.PSBlendMode)Enum.Parse(typeof(SprueKit.Data.PSBlendMode), thisElem.GetStringElement("BlendMode"));
            BlendSource = (SprueKit.Data.PSAlphaMode)Enum.Parse(typeof(SprueKit.Data.PSAlphaMode), thisElem.GetStringElement("BlendSource"));
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write((int)BlendMode);
            strm.Write((int)BlendSource);
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            BlendMode = (SprueKit.Data.PSBlendMode)strm.ReadInt32();
            BlendSource = (SprueKit.Data.PSAlphaMode)strm.ReadInt32();
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            BlendNode retVal = new BlendNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            BlendNode retVal = into as BlendNode;
            retVal.blendMode_ = this.blendMode_;
            retVal.alphaMode_ = this.alphaMode_;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }    
}

namespace SprueKit.Data.TexGen
{
    public partial class Paint2DNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Version", Version.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
            PermutationSerialization.SerializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Version = thisElem.GetIntElement("Version");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
            PermutationSerialization.DeserializePermutations(ctx, thisElem, this, Permutations);
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Version);
            strm.Write(Name ?? "");
            strm.Write(Description ?? "");
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
            PermutationSerialization.SerializePermutations(ctx, strm, this, Permutations);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Version = strm.ReadInt32();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
            PermutationSerialization.DeserializePermutations(ctx, strm, this, Permutations);
        }
        public override Data.Graph.GraphNode Clone()
        {
            Paint2DNode retVal = new Paint2DNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            Paint2DNode retVal = into as Paint2DNode;
            retVal.lastExecutionContext = this.lastExecutionContext;
        }
    }
}
