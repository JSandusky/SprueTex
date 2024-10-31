using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;
using Matrix = Microsoft.Xna.Framework.Matrix;
using SprueKit.Data.Graph;
using System.Xml;
using System.IO;

namespace SprueKit.Data.ShaderGen
{
    public partial class FloatUniformNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("DefaultValue", DefaultValue.ToString());
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            DefaultValue = thisElem.GetFloatElement("DefaultValue");
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(DefaultValue);
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            DefaultValue = strm.ReadSingle();
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            FloatUniformNode retVal = new FloatUniformNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            FloatUniformNode retVal = into as FloatUniformNode;
            retVal.defaultValue_ = this.defaultValue_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class Float2UniformNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("DefaultValue", DefaultValue.ToTightString());
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            DefaultValue = thisElem.GetStringElement("DefaultValue").ToVector2();
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(DefaultValue);
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            DefaultValue = strm.ReadVector2();
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            Float2UniformNode retVal = new Float2UniformNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            Float2UniformNode retVal = into as Float2UniformNode;
            retVal.defaultValue_ = this.defaultValue_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class Float3UniformNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("DefaultValue", DefaultValue.ToTightString());
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            DefaultValue = thisElem.GetStringElement("DefaultValue").ToVector3();
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(DefaultValue);
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            DefaultValue = strm.ReadVector3();
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            Float3UniformNode retVal = new Float3UniformNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            Float3UniformNode retVal = into as Float3UniformNode;
            retVal.defaultValue_ = this.defaultValue_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class Float4UniformNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("DefaultValue", DefaultValue.ToTightString());
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            DefaultValue = thisElem.GetStringElement("DefaultValue").ToVector4();
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(DefaultValue);
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            DefaultValue = strm.ReadVector4();
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            Float4UniformNode retVal = new Float4UniformNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            Float4UniformNode retVal = into as Float4UniformNode;
            retVal.defaultValue_ = this.defaultValue_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class MatrixUniformNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("DefaultValue", DefaultValue.ToTightString());
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            DefaultValue = thisElem.GetStringElement("DefaultValue").ToMatrix();
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(DefaultValue);
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            DefaultValue = strm.ReadMatrix();
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            MatrixUniformNode retVal = new MatrixUniformNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            MatrixUniformNode retVal = into as MatrixUniformNode;
            retVal.defaultValue_ = this.defaultValue_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class PiNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            PiNode retVal = new PiNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            PiNode retVal = into as PiNode;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class TauNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            TauNode retVal = new TauNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            TauNode retVal = into as TauNode;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class PhiNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            PhiNode retVal = new PhiNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            PhiNode retVal = into as PhiNode;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class Root2Node
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            Root2Node retVal = new Root2Node();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            Root2Node retVal = into as Root2Node;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class EulersConstantNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            EulersConstantNode retVal = new EulersConstantNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            EulersConstantNode retVal = into as EulersConstantNode;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class IntConstant
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Value", Value.ToString());
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Value = thisElem.GetIntElement("Value");
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Value);
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Value = strm.ReadInt32();
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            IntConstant retVal = new IntConstant();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            IntConstant retVal = into as IntConstant;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class FloatConstant
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Value", Value.ToString());
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Value = thisElem.GetFloatElement("Value");
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Value);
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Value = strm.ReadSingle();
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            FloatConstant retVal = new FloatConstant();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            FloatConstant retVal = into as FloatConstant;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class Float2Constant
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Value", Value.ToTightString());
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Value = thisElem.GetStringElement("Value").ToVector2();
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Value);
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Value = strm.ReadVector2();
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            Float2Constant retVal = new Float2Constant();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            Float2Constant retVal = into as Float2Constant;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class Float3Constant
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Value", Value.ToTightString());
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Value = thisElem.GetStringElement("Value").ToVector3();
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Value);
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Value = strm.ReadVector3();
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            Float3Constant retVal = new Float3Constant();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            Float3Constant retVal = into as Float3Constant;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class Float4Constant
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("Value", Value.ToTightString());
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            Value = thisElem.GetStringElement("Value").ToVector4();
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(Value);
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            Value = strm.ReadVector4();
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            Float4Constant retVal = new Float4Constant();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            Float4Constant retVal = into as Float4Constant;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class AddNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
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
            retVal.opChar = this.opChar;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class SubtractNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
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
            retVal.opChar = this.opChar;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class MultiplyNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
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
            retVal.opChar = this.opChar;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class DivideNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
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
            retVal.opChar = this.opChar;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class CosNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
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
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class AcosNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            AcosNode retVal = new AcosNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            AcosNode retVal = into as AcosNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class SinNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
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
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class AsinNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            AsinNode retVal = new AsinNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            AsinNode retVal = into as AsinNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class TanNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
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
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class AtanNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            AtanNode retVal = new AtanNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            AtanNode retVal = into as AtanNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class Atan2Node
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            Atan2Node retVal = new Atan2Node();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            Atan2Node retVal = into as Atan2Node;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class PowNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
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
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class ExpNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
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
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class Exp2Node
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            Exp2Node retVal = new Exp2Node();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            Exp2Node retVal = into as Exp2Node;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class LogNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            LogNode retVal = new LogNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            LogNode retVal = into as LogNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class Log2Node
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            Log2Node retVal = new Log2Node();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            Log2Node retVal = into as Log2Node;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class SqrtNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
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
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class RsqrtNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            RsqrtNode retVal = new RsqrtNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            RsqrtNode retVal = into as RsqrtNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class AbsNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            AbsNode retVal = new AbsNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            AbsNode retVal = into as AbsNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class SignNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            SignNode retVal = new SignNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            SignNode retVal = into as SignNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class FloorNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            FloorNode retVal = new FloorNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            FloorNode retVal = into as FloorNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class CeilNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            CeilNode retVal = new CeilNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            CeilNode retVal = into as CeilNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class FracNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            FracNode retVal = new FracNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            FracNode retVal = into as FracNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class FmodNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            FmodNode retVal = new FmodNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            FmodNode retVal = into as FmodNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class RoundNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            RoundNode retVal = new RoundNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            RoundNode retVal = into as RoundNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class MinNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            MinNode retVal = new MinNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            MinNode retVal = into as MinNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class MaxNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            MaxNode retVal = new MaxNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            MaxNode retVal = into as MaxNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class ClampNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            ClampNode retVal = new ClampNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            ClampNode retVal = into as ClampNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class LerpNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            LerpNode retVal = new LerpNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            LerpNode retVal = into as LerpNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class StepNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            StepNode retVal = new StepNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            StepNode retVal = into as StepNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class SmoothstepNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            SmoothstepNode retVal = new SmoothstepNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            SmoothstepNode retVal = into as SmoothstepNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class TruncNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            TruncNode retVal = new TruncNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            TruncNode retVal = into as TruncNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class SaturateNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            SaturateNode retVal = new SaturateNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            SaturateNode retVal = into as SaturateNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class DotNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            DotNode retVal = new DotNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            DotNode retVal = into as DotNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class CrossNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            CrossNode retVal = new CrossNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            CrossNode retVal = into as CrossNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class NormalizeNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            NormalizeNode retVal = new NormalizeNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            NormalizeNode retVal = into as NormalizeNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class LengthNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            LengthNode retVal = new LengthNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            LengthNode retVal = into as LengthNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class DistanceNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            DistanceNode retVal = new DistanceNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            DistanceNode retVal = into as DistanceNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class DstNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            DstNode retVal = new DstNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            DstNode retVal = into as DstNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class ReflectNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            ReflectNode retVal = new ReflectNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            ReflectNode retVal = into as ReflectNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class RefractNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            RefractNode retVal = new RefractNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            RefractNode retVal = into as RefractNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class FaceforwardNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            FaceforwardNode retVal = new FaceforwardNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            FaceforwardNode retVal = into as FaceforwardNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class TransformNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            TransformNode retVal = new TransformNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            TransformNode retVal = into as TransformNode;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class TransformNormalNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            TransformNormalNode retVal = new TransformNormalNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            TransformNormalNode retVal = into as TransformNormalNode;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class TransposeNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            TransposeNode retVal = new TransposeNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            TransposeNode retVal = into as TransposeNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class DeterminantNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            DeterminantNode retVal = new DeterminantNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            DeterminantNode retVal = into as DeterminantNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class DdxNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            DdxNode retVal = new DdxNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            DdxNode retVal = into as DdxNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class DdyNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            DdyNode retVal = new DdyNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            DdyNode retVal = into as DdyNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class IsinfiniteNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            IsinfiniteNode retVal = new IsinfiniteNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            IsinfiniteNode retVal = into as IsinfiniteNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class IsnanNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            IsnanNode retVal = new IsnanNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            IsnanNode retVal = into as IsnanNode;
            retVal.funcName_ = this.funcName_;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class VertexShaderOutputNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            VertexShaderOutputNode retVal = new VertexShaderOutputNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            VertexShaderOutputNode retVal = into as VertexShaderOutputNode;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class PixelShaderOutputNode
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            PixelShaderOutputNode retVal = new PixelShaderOutputNode();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            PixelShaderOutputNode retVal = into as PixelShaderOutputNode;
        }
    }
}
namespace SprueKit.Data.ShaderGen
{
    public partial class InputData
    {
        public override void SerializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            thisElem.AddStringElement("IsValid", IsValid.ToString());
            thisElem.AddStringElement("Name", Name);
            thisElem.AddStringElement("Description", Description);
            thisElem.AddStringElement("NodeID", NodeID.ToString());
            thisElem.AddStringElement("VisualX", VisualX.ToString());
            thisElem.AddStringElement("VisualY", VisualY.ToString());
            thisElem.AddStringElement("EntryPoint", EntryPoint.ToString());
            thisElem.AddStringElement("EventPoint", EventPoint.ToString());
        }
        public override void DeserializeProperties(SerializationContext ctx, XmlElement thisElem)
        {
            IsValid = thisElem.GetBoolElement("IsValid");
            Name = thisElem.GetStringElement("Name");
            Description = thisElem.GetStringElement("Description");
            NodeID = thisElem.GetIntElement("NodeID");
            VisualX = (double)thisElem.GetFloatElement("VisualX");
            VisualY = (double)thisElem.GetFloatElement("VisualY");
            EntryPoint = thisElem.GetBoolElement("EntryPoint");
            EventPoint = thisElem.GetBoolElement("EventPoint");
        }
        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            strm.Write(IsValid);
            strm.Write(Name);
            strm.Write(Description);
            strm.Write(NodeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
            strm.Write(EntryPoint);
            strm.Write(EventPoint);
        }
        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            IsValid = strm.ReadBoolean();
            Name = strm.ReadString();
            Description = strm.ReadString();
            NodeID = strm.ReadInt32();
            VisualX = strm.ReadDouble();
            VisualY = strm.ReadDouble();
            EntryPoint = strm.ReadBoolean();
            EventPoint = strm.ReadBoolean();
        }
        public override Data.Graph.GraphNode Clone()
        {
            InputData retVal = new InputData();
            CloneFields(retVal);
            return retVal;
        }
        protected override void CloneFields(Data.Graph.GraphNode into)
        {
            base.CloneFields(into);
            InputData retVal = into as InputData;
        }
    }
}
