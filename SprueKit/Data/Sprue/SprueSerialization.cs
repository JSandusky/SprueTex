using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Collections.ObjectModel;

namespace SprueKit.Data
{
    public partial class SpruePiece
    {
        public virtual void Serialize(SerializationContext context, BinaryWriter writer)
        {
            Serialize(context, Children, writer);
            Serialize(context, Components, writer);
            PermutationSerialization.SerializePermutations(context, writer, this, Permutations);
        }

        public virtual void Serialize(SerializationContext context, XmlElement element)
        {
            Serialize(context, Children, element, "children");
            Serialize(context, Components, element, "components");
            PermutationSerialization.SerializePermutations(context, element, this, Permutations);
        }

        protected void SerializeBaseProperties(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(LocalPosition);
            writer.Write(LocalRotation);
            writer.Write(LocalScale);
            writer.Write(IsEnabled);
            writer.Write(Capabilities);
            writer.Write(Flags);
            writer.Write((int)Symmetric);
        }

        protected void SerializeBaseProperties(XmlElement into)
        {
            into.AddStringElement("name", Name);
            into.AddStringElement("position", LocalPosition.ToTightString());
            into.AddStringElement("rotation", LocalRotation.ToTightString());
            into.AddStringElement("scale", LocalScale.ToTightString());
            into.AddStringElement("is_enabled", IsEnabled.ToString());
            into.AddStringElement("capabilities", Capabilities.ToString());
            into.AddStringElement("flags", Flags.ToString());
            into.AddEnumElement("symmetric", Symmetric);
        }

        public virtual void Deserialize(SerializationContext context, BinaryReader reader)
        {
            Deserialize(context, Children, reader);
            Deserialize(context, Components, reader);
            PermutationSerialization.DeserializePermutations(context, reader, this, Permutations);
        }

        public virtual void Deserialize(SerializationContext context, XmlElement element)
        {
            Deserialize(context, Children, element, "children");
            Deserialize(context, Components, element, "components");
            PermutationSerialization.DeserializePermutations(context, element, this, Permutations);
        }

        protected void DeserializeBaseProperties(BinaryReader reader)
        {
            name_ = reader.ReadString();
            position_ = reader.ReadVector3();
            rotation_ = reader.ReadQuaternion();
            scale_ = reader.ReadVector3();
            isEnabled_ = reader.ReadBoolean();
            capabilities_ = reader.ReadUInt32();
            flags_ = reader.ReadUInt32();
            symmetric_ = (SymmetricAxis)reader.ReadInt32();
        }

        protected void DeserializeBaseProperties(XmlElement from)
        {
            name_ = from.GetStringElement("name");
            position_ = from.GetStringElement("position", "").ToVector3();
            rotation_ = from.GetStringElement("rotation", "").ToQuaternion();
            scale_ = from.GetStringElement("scale", "").ToVector3();
            isEnabled_ = from.GetBoolElement("is_enabled");
            capabilities_ = from.GetUIntElement("capabilities");
            flags_ = from.GetUIntElement("flags");
            symmetric_ = from.GetEnumElement("symmetric", SymmetricAxis.None);
        }

        protected void Deserialize<T>(SerializationContext context, ObservableCollection<T> collection, BinaryReader reader) where T : SpruePiece
        {
            int itemCount = reader.ReadInt32();
            for (int i = 0; i < itemCount; ++i)
            {
                string typeName = reader.ReadString();
                Type t = Type.GetType(typeName);
                T o = Activator.CreateInstance(t) as T;
                o.Deserialize(context, reader);
                collection.Add(o);
            }
        }

        protected void Deserialize<T>(SerializationContext context, ObservableCollection<T> collection, XmlElement parentElement, string targetName) where T : SpruePiece
        {
            var collectionNode = parentElement.SelectSingleNode(targetName);
            if (collectionNode != null)
            {
                var collectionElem = collectionNode as XmlElement;
                for (int i = 0; i < collectionElem.ChildNodes.Count; ++i)
                {
                    var subElem = collectionElem.ChildNodes[i] as XmlElement;
                    if (subElem != null)
                    {
                        Type t = Type.GetType(subElem.GetAttribute("type"));
                        T o = Activator.CreateInstance(t) as T;
                        o.Deserialize(context, subElem);
                        collection.Add(o);
                    }
                }
            }
        }

        protected virtual void Serialize(SerializationContext context, IEnumerable<SpruePiece> pieces, BinaryWriter writer)
        {
            writer.Write(pieces.Count());
            foreach (var piece in pieces)
            {
                if (piece.IsLocked)
                    continue;
                writer.Write(piece.GetType().FullName);
                piece.Serialize(context, writer);
            }
        }

        protected virtual void Serialize(SerializationContext context, IEnumerable<SpruePiece> pieces, XmlElement parentElement, string targetName)
        {
            if (pieces.Count() > 0)
            {
                XmlElement childrenElem = parentElement.OwnerDocument.CreateElement(targetName);
                parentElement.AppendChild(childrenElem);

                foreach (var piece in pieces)
                {
                    if (piece.IsLocked)
                        continue;
                    XmlElement childElem = parentElement.OwnerDocument.CreateElement("object");
                    childrenElem.AppendChild(childElem);
                    childElem.SetAttribute("type", piece.GetType().FullName);
                    piece.Serialize(context, childElem);
                }
            }
        }
    }

    #region Modeling Parts

    public partial class SprueModel
    {
        static string FOURCC = "SPRE";

        public void SaveTo(SerializationContext context, BinaryWriter writer)
        {
            writer.Write(FourCC.ToFourCC(FOURCC)); // magic
            writer.Write((int)1); // verison
            Serialize(context, writer);
        }

        public void SaveTo(SerializationContext context, XmlDocument document)
        {
            var myElem = document.DocumentElement.CreateChild("spruemodel");
            Serialize(context, myElem);
        }

        public static SprueModel ReadFrom(SerializationContext context, BinaryReader reader)
        {
            SprueModel ret = new SprueModel();

            int fourcc = reader.ReadInt32();

            if (FourCC.VerifyFourCC(fourcc, "SPRE"))
                ret.Deserialize(context, reader);

            return ret;
        }

        public static SprueModel ReadFrom(SerializationContext context, XmlElement element)
        {
            if (element.Name.ToLower().Equals("spruemodel"))
            {
                SprueModel ret = new SprueModel();
                ret.Deserialize(context, element);
                return ret;
            }
            throw new Exception("Failed to read XML document");
        }

        public override void Serialize(SerializationContext context, BinaryWriter writer)
        {
            SerializeBaseProperties(writer);
            base.Serialize(context, writer);
        }

        public override void Deserialize(SerializationContext context, BinaryReader reader)
        {
            DeserializeBaseProperties(reader);
            base.Deserialize(context, reader);
        }

        public override void Serialize(SerializationContext context, XmlElement element)
        {
            SerializeBaseProperties(element);
            element.AddStringElement("texture_size", textureSize_.ToTightString());
            element.AddStringElement("pack_texture_size", packedTextureSize_.ToTightString());
            element.AddStringElement("pack", packMeshes_.ToString());
            element.AddStringElement("generate_report", generateReport_.ToString());
            element.AddStringElement("bone_falloff", weightsCurve_.ToString());
            element.AddStringElement("use_bmesh", useBMesh_.ToString());
            element.AddStringElement("bmesh_suvdivs", bmeshSubdivs_.ToString());
            base.Serialize(context, element);
        }

        public override void Deserialize(SerializationContext context, XmlElement element)
        {
            DeserializeBaseProperties(element);
            textureSize_ = element.GetStringElement("texture_size", "1024 1024").ToIntVector2();
            packedTextureSize_ = element.GetStringElement("pack_texture_size", "1024 1024").ToIntVector2();
            packMeshes_ = element.GetBoolElement("pack", true);
            generateReport_ = element.GetBoolElement("generate_report", true);
            weightsCurve_ = element.GetStringElement("bone_falloff", "Logistic 0 0 1 1").ToResponseCurve();
            useBMesh_ = element.GetBoolElement("use_bmesh", false);
            bmeshSubdivs_ = element.GetIntElement("bmesh_subdivs", 1);
            base.Deserialize(context, element);
        }

        public static SprueModel LoadFile(string filePath)
        {
            SprueModel ret = null;
            SerializationContext ctx = new SerializationContext(new Uri(System.IO.Path.GetDirectoryName(filePath)));
            if (filePath.EndsWith(".xml"))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filePath);
                ret = SprueModel.ReadFrom(ctx, doc.DocumentElement.SelectSingleNode("spruemodel") as XmlElement);
            }
            else if (filePath.EndsWith(".sprm"))
            {
                using (System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open))
                {
                    using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream))
                    {
                        ret = SprueModel.ReadFrom(ctx, reader);
                    }
                }
            }

            if (ctx.BrokenPaths.Count > 0)
            {
                Dlg.PathFixupDlg dlg = new Dlg.PathFixupDlg(ctx);
                dlg.ShowDialog();
            }
            return ret;
        }
    }

    public partial class SimplePiece
    {
        public override void Serialize(SerializationContext context, BinaryWriter writer)
        {
            SerializeBaseProperties(writer);
            writer.Write((int)CSGOperation);
            writer.Write((int)ShapeType);
            writer.Write(Params);
            base.Serialize(context, writer);
        }

        public override void Deserialize(SerializationContext context, BinaryReader reader)
        {
            DeserializeBaseProperties(reader);
            csgOp_ = (CSGOperation)reader.ReadInt32();
            type_ = (ShapeFunctionType)reader.ReadInt32();
            params_ = reader.ReadVector4();
            base.Deserialize(context, reader);
        }

        public override void Serialize(SerializationContext context, XmlElement element)
        {
            SerializeBaseProperties(element);
            element.AddEnumElement("csg_operation", CSGOperation);
            element.AddEnumElement("shape", ShapeType);
            element.AddStringElement("params", Params.ToTightString());
            base.Serialize(context, element);
        }

        public override void Deserialize(SerializationContext context, XmlElement element)
        {
            DeserializeBaseProperties(element);
            csgOp_ = element.GetEnumElement("csg_operation", CSGOperation.Add);
            type_ = element.GetEnumElement("shape", ShapeFunctionType.Sphere);
            params_ = element.GetStringElement("params", "").ToVector4();
            base.Deserialize(context, element);
        }
    }

    public partial class ChainPiece
    {
        public override void Serialize(SerializationContext context, BinaryWriter writer)
        {
            SerializeBaseProperties(writer);
            writer.Write(SmoothingLevels);
            writer.Write(IsSpine);
            Serialize(context, Bones, writer);
            base.Serialize(context, writer);
        }

        public override void Deserialize(SerializationContext context, BinaryReader reader)
        {
            DeserializeBaseProperties(reader);
            smoothingLevels_ = reader.ReadUInt32();
            spine_ = reader.ReadBoolean();
            Deserialize(context, Bones, reader);
            base.Deserialize(context, reader);
        }

        public override void Serialize(SerializationContext context, XmlElement element)
        {
            SerializeBaseProperties(element);
            element.AddStringElement("smoothing_levels", SmoothingLevels.ToString());
            element.AddStringElement("is_spine", IsSpine.ToString());
            Serialize(context, Bones, element, "bones");
            base.Serialize(context, element);
        }

        public override void Deserialize(SerializationContext context, XmlElement element)
        {
            DeserializeBaseProperties(element);
            smoothingLevels_ = element.GetUIntElement("smoothing_levels");
            spine_ = element.GetBoolElement("is_spine", false);
            Deserialize(context, Bones, element, "bones");
            base.Deserialize(context, element);
        }

        public partial class ChainBone
        {
            public override void Serialize(SerializationContext context, BinaryWriter writer)
            {
                SerializeBaseProperties(writer);
                writer.Write(CrossSection);
                base.Serialize(context, writer);
            }

            public override void Deserialize(SerializationContext context, BinaryReader reader)
            {
                DeserializeBaseProperties(reader);
                crossSection_ = reader.ReadVector3();
                base.Deserialize(context, reader);
            }

            public override void Serialize(SerializationContext context, XmlElement element)
            {
                SerializeBaseProperties(element);
                element.AddStringElement("cross_section", CrossSection.ToTightString());
                base.Serialize(context, element);
            }

            public override void Deserialize(SerializationContext context, XmlElement element)
            {
                DeserializeBaseProperties(element);
                crossSection_ = element.GetStringElement("cross_section", "").ToVector3();
                base.Deserialize(context, element);
            }
        }
    }

    public partial class ModelPiece
    {
        public override void Serialize(SerializationContext context, BinaryWriter writer)
        {
            SerializeBaseProperties(writer);
            writer.Write(IsDetail);
            writer.Write((int)Combine);
            if (ModelFile.ModelFile != null)
                writer.Write(context.GetRelativePath(ModelFile.ModelFile).ToString());
            else
                writer.Write("");
            base.Serialize(context, writer);
        }

        public override void Deserialize(SerializationContext context, BinaryReader reader)
        {
            DeserializeBaseProperties(reader);
            IsDetail = reader.ReadBoolean();
            combination_ = (SprueBindings.CSGTask)reader.ReadInt32();
            base.Deserialize(context, reader);
        }

        public override void Serialize(SerializationContext context, XmlElement element)
        {
            SerializeBaseProperties(element);
            element.AddStringElement("is_detail", IsDetail.ToString());
            element.AddEnumElement("combine", combination_);
            if (ModelFile != null && ModelFile.ModelFile != null)
            {
                var mdlElem = element.CreateChild("model");
                ModelFile.Write(context, mdlElem);
            }
            base.Serialize(context, element);
        }

        public override void Deserialize(SerializationContext context, XmlElement element)
        {
            DeserializeBaseProperties(element);
            isDetail_ = element.GetBoolElement("is_detail");
            combination_ = element.GetEnumElement("combine", SprueBindings.CSGTask.Merge);
            var mdlElem = element.SelectSingleNode("model") as XmlElement;
            if (mdlElem != null)
                this.ModelFile.Read(context, mdlElem);
            base.Deserialize(context, element);
        }
    }

    public partial class InstancePiece
    {
        public override void Serialize(SerializationContext context, BinaryWriter writer)
        {
            SerializeBaseProperties(writer);
            writer.Write(filePath_);
            base.Serialize(context, writer);
        }

        public override void Deserialize(SerializationContext context, BinaryReader reader)
        {
            DeserializeBaseProperties(reader);
            SprueModel = reader.ReadUri();
            base.Deserialize(context, reader);
        }

        public override void Serialize(SerializationContext context, XmlElement element)
        {
            SerializeBaseProperties(element);
            if (filePath_ != null && System.IO.File.Exists(filePath_.AbsolutePath))
                element.AddStringElement("model", context.GetRelativePathString(filePath_));
            base.Serialize(context, element);
        }

        public override void Deserialize(SerializationContext context, XmlElement element)
        {
            DeserializeBaseProperties(element);
            string fileString = element.GetStringElement("model");
            Uri result = context.GetAbsolutePath(new Uri(fileString), this, "SprueModel", "SprueModel", FileData.SprueModelMask);
            if (result != null)
                SprueModel = result;
            base.Deserialize(context, element);
        }
    }

    public partial class MarkerPiece
    {
        public override void Serialize(SerializationContext context, BinaryWriter writer)
        {
            SerializeBaseProperties(writer);
            writer.Write(ShowForward);
            base.Serialize(context, writer);
        }

        public override void Deserialize(SerializationContext context, BinaryReader reader)
        {
            DeserializeBaseProperties(reader);
            showForward_ = reader.ReadBoolean();
            base.Deserialize(context, reader);
        }

        public override void Serialize(SerializationContext context, XmlElement element)
        {
            SerializeBaseProperties(element);
            element.AddStringElement("show_forward", showForward_.ToString());
            base.Serialize(context, element);
        }

        public override void Deserialize(SerializationContext context, XmlElement element)
        {
            DeserializeBaseProperties(element);
            showForward_ = element.GetBoolElement("show_forward");
            base.Deserialize(context, element);
        }
    }

    #endregion

    public partial class TextureComponent
    {

    }

    /// <summary>
    /// Also covers decal, box, cylinder, and dome
    /// </summary>
    public partial class BasicTextureComponent
    {
        protected void SerializeTextures(SerializationContext context, XmlElement element)
        {
            var texturesElem = element.CreateChild("textures");
            foreach (var texture in this.TextureMaps)
                texture.Write(context, texturesElem);
        }


        protected void DeserializeTextures(SerializationContext context, XmlElement from)
        {
            var texturesElem = from.SelectSingleNode("textures");
            if (texturesElem != null)
            {
                var nodes = texturesElem.SelectNodes("TextureMap");
                for (int i = 0; i < nodes.Count; ++i)
                {
                    var texMap = new TextureMap();
                    texMap.Read(context, nodes[i] as XmlElement);
                    this.TextureMaps.Add(texMap);
                }
            }
        }

        public override void Serialize(SerializationContext context, BinaryWriter writer)
        {
            SerializeBaseProperties(writer);
            writer.Write(Tiling);
            base.Serialize(context, writer);
        }

        public override void Deserialize(SerializationContext context, BinaryReader reader)
        {
            DeserializeBaseProperties(reader);
            tiling_ = reader.ReadVector2();
            base.Deserialize(context, reader);
        }

        public override void Serialize(SerializationContext context, XmlElement element)
        {
            SerializeBaseProperties(element);
            element.AddStringElement("uv_tiling", tiling_.ToTightString());
            SerializeTextures(context, element);
            base.Serialize(context, element);
        }

        public override void Deserialize(SerializationContext context, XmlElement element)
        {
            DeserializeBaseProperties(element);
            tiling_ = element.GetStringElement("uv_tiling", "").ToVector2();
            DeserializeTextures(context, element);
            base.Deserialize(context, element);
        }
    }

    public partial class DecalTextureComponent
    {
        public override void Serialize(SerializationContext context, XmlElement element)
        {
            SerializeBaseProperties(element);
            element.AddStringElement("uv_tiling", tiling_.ToTightString());
            element.AddStringElement("normal_tolerance", normalTolerance_.ToString());
            element.AddStringElement("pass_through", passThrough_.ToString());
            SerializeTextures(context, element);
            base.Serialize(context, element);
        }

        public override void Deserialize(SerializationContext context, XmlElement element)
        {
            DeserializeBaseProperties(element);
            tiling_ = element.GetStringElement("uv_tiling", "").ToVector2();
            normalTolerance_ = element.GetFloatElement("normal_tolerance", 0.25f);
            passThrough_ = element.GetBoolElement("pass_through", false);
            DeserializeTextures(context, element);
            base.Deserialize(context, element);
        }
    }

    public partial class GradientTextureComponent
    {
        public override void Serialize(SerializationContext context, BinaryWriter writer)
        {
            SerializeBaseProperties(writer);
            writer.Write(lowerColor_);
            writer.Write(upperColor_);
            base.Serialize(context, writer);
        }

        public override void Deserialize(SerializationContext context, BinaryReader reader)
        {
            DeserializeBaseProperties(reader);
            lowerColor_ = reader.ReadColor();
            upperColor_ = reader.ReadColor();
            base.Deserialize(context, reader);
        }

        public override void Serialize(SerializationContext context, XmlElement element)
        {
            SerializeBaseProperties(element);
            element.AddStringElement("lower_color", lowerColor_.ToString());
            element.AddStringElement("upper_color", upperColor_.ToString());
            base.Serialize(context, element);
        }

        public override void Deserialize(SerializationContext context, XmlElement element)
        {
            DeserializeBaseProperties(element);
            lowerColor_ = element.GetStringElement("lower_color", "").ToColor();
            upperColor_ = element.GetStringElement("upper_color", "").ToColor();
            base.Deserialize(context, element);
        }
    }

    public partial class ColorCubeTextureComponent
    {
        public override void Serialize(SerializationContext context, BinaryWriter writer)
        {
            SerializeBaseProperties(writer);
            for (int i = 0; i < 6; ++i)
                writer.Write(colors_[i]);
            base.Serialize(context, writer);
        }

        public override void Deserialize(SerializationContext context, BinaryReader reader)
        {
            DeserializeBaseProperties(reader);
            for (int i = 0; i < 6; ++i)
                colors_[i] = reader.ReadColor();
            base.Deserialize(context, reader);
        }

        public override void Serialize(SerializationContext context, XmlElement element)
        {
            SerializeBaseProperties(element);
            element.AddStringElement("pos_x_color", PositiveXColor.ToTightString());
            element.AddStringElement("neg_x_color", NegativeXColor.ToTightString());
            element.AddStringElement("pos_y_color", PositiveYColor.ToTightString());
            element.AddStringElement("neg_y_color", NegativeYColor.ToTightString());
            element.AddStringElement("pos_z_color", PositiveZColor.ToTightString());
            element.AddStringElement("neg_z_color", NegativeZColor.ToTightString());
            base.Serialize(context, element);
        }

        public override void Deserialize(SerializationContext context, XmlElement element)
        {
            DeserializeBaseProperties(element);
            colors_[0] = element.GetStringElement("pos_x_color", "").ToColor();
            colors_[1] = element.GetStringElement("neg_x_color", "").ToColor();
            colors_[2] = element.GetStringElement("pos_y_color", "").ToColor();
            colors_[3] = element.GetStringElement("neg_y_color", "").ToColor();
            colors_[4] = element.GetStringElement("pos_z_color", "").ToColor();
            colors_[5] = element.GetStringElement("neg_z_color", "").ToColor();
            base.Deserialize(context, element);
        }
    }

    public partial class DecalStripTextureComponent
    {

    }
}
