using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SprueKit.Graphics;

using Microsoft.Xna.Framework;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace SprueKit.Data
{
    public partial class TextureComponent : SprueComponent
    {
        public virtual Color? Sample(Vector3 position, Vector3 normal, SprueBindings.ImageData image)
        {
            return null;
        }
    }

    public partial class BasicTextureComponent : TextureComponent
    {
        protected Vector2 tiling_ = new Vector2(1, 1);

        [Category("Texture")]
        public Vector2 Tiling { get { return tiling_; } set { tiling_ = value; OnPropertyChanged(); } }

        [Category("Texture")]
        public ObservableCollection<TextureMap> TextureMaps { get; set; } = new ObservableCollection<TextureMap>();
    }

    public partial class BoxTextureComponent : BasicTextureComponent
    {
        bool triplanarSampling_ = true;

        [Category("Texture")]
        public bool UseTriplanarSampling { get { return triplanarSampling_; } set { triplanarSampling_ = value; OnPropertyChanged(); } }

        public override void DebugDraw(DebugRenderer renderer, DebugDrawMode mode)
        {
            renderer.DrawWireBox(Transform, 1, 1, 1, Color.Green);
        }

        public override Color? Sample(Vector3 position, Vector3 normal, SprueBindings.ImageData image)
        {
            if (image == null)
                return null;

            var trans = InverseTransform;
            Vector3 localPos = Vector3.Transform(position, trans);
            Vector3 localNormal = Vector3.TransformNormal(normal, trans);
            BoundingBox bounds = new BoundingBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));
            if (bounds.Contains(localPos) == ContainmentType.Contains)
            {
                if (UseTriplanarSampling)
                {
                    Vector3 interpolatedNormal = localNormal.Abs();
                    float weightSum = interpolatedNormal.X + interpolatedNormal.Y + interpolatedNormal.Z;
                    interpolatedNormal /= weightSum;

                    Vector3 scaling = new Vector3(1, 1, 1);

                    Vector2 coord1 = new Vector2(localPos.Y, localPos.Z);// * scaling.X;
                    Vector2 coord2 = new Vector2(localPos.X, localPos.Z);// * scaling.Y;
                    Vector2 coord3 = new Vector2(localPos.X, localPos.Y);// * scaling.Z;

                    Color col1 = image.GetPixelBilinear(coord1.X / scaling.X, coord1.Y / scaling.Y);
                    Color col2 = image.GetPixelBilinear(coord2.X / scaling.X, coord2.Y / scaling.Y);
                    Color col3 = image.GetPixelBilinear(coord3.X / scaling.X, coord3.Y / scaling.Y);

                    Color writeColor = col1.Mul(interpolatedNormal.X).Add(col2.Mul(interpolatedNormal.Y)).Add(col3.Mul(interpolatedNormal.Z));
                    writeColor.A = 255;
                    return writeColor;
                }
                else
                {
                // cubemap style mapping
                    Vector3 absVec = localNormal.Abs();
                    int maxElem = absVec.MaxElementIndex();
                    Vector2 coord = Vector2.Zero;

                    // Normalized position indicates the sampling coordinates
                    var normalizedPosition = bounds.NormalizedPosition(localPos);

                    if (maxElem == 0) // X axis
                        coord = new Vector2(normalizedPosition.Z, normalizedPosition.Y);
                    else if (maxElem == 1) // Y axis
                        coord = new Vector2(normalizedPosition.X, normalizedPosition.Z);
                    else if (maxElem == 2) // Z axis
                        coord = new Vector2(normalizedPosition.X, normalizedPosition.Y);

                    return image.GetPixelBilinear(coord.X, coord.Y);
                }
            }
            return null;
        }
    }

    public partial class DecalTextureComponent : BasicTextureComponent
    {
        float normalTolerance_ = 0.25f;
        bool passThrough_ = false;

        [Category("Texture")]
        [Description("Acceptable range for passing pixels based on surface normal, 1.0 must perfectly point at the decal and 0.0 may be perpendicular")]
        public float NormalTolerance { get { return normalTolerance_; } set { normalTolerance_ = value; OnPropertyChanged(); } }

        [Category("Texture")]
        [Description("Texture will be projected onto back-facing polygons as well as front-facing")]
        public bool PassThrough { get { return passThrough_; } set { passThrough_ = value; OnPropertyChanged(); } }

        static Vector3[] quadVerts =
        {
            new Vector3(-1,1, 0), //topleft
            new Vector3(1, 1, 0), //topright
            new Vector3(1,-1, 0), //bottomright
            new Vector3(-1, -1, 0), //bottom left
        };
        public override void DebugDraw(DebugRenderer renderer, DebugDrawMode mode)
        {
            renderer.DrawLine(Vector3.Transform(quadVerts[0], Transform),
                Vector3.Transform(quadVerts[1], Transform), Color.Green);
            renderer.DrawLine(Vector3.Transform(quadVerts[2], Transform),
                Vector3.Transform(quadVerts[3], Transform), Color.Green);
            renderer.DrawLine(Vector3.Transform(quadVerts[0], Transform),
                Vector3.Transform(quadVerts[3], Transform), Color.Green);
            renderer.DrawLine(Vector3.Transform(quadVerts[1], Transform),
                Vector3.Transform(quadVerts[2], Transform), Color.Green);

            for (int i = 0; i < 4; ++i)
                renderer.DrawLine(Vector3.Transform(quadVerts[i], Transform),
                    Vector3.Transform(quadVerts[i] + Vector3.UnitZ, Transform), Color.CornflowerBlue);            
        }

        public override Color? Sample(Vector3 position, Vector3 normal, SprueBindings.ImageData image)
        {
            if (image == null)
                return null;

            var trans = InverseTransform;
            var localPosition = Vector3.Transform(position, trans);
            var localNormal = Vector3.TransformNormal(normal, trans);
            localNormal.Normalize();
            float dotValue = -Vector3.Dot(localNormal, Vector3.UnitZ);
            if (dotValue < 0 && !passThrough_)
                return null;
            if (Mathf.Abs(dotValue) < normalTolerance_)
                return null;
            if (localPosition.Z < 0)
                return null;

            Vector3 topLeft = new Vector3(-1, 1, 0);
            Plane plane = XNAExt.PlaneFromPointNormal(topLeft, Vector3.UnitZ);            
            Vector3 planePos = plane.Project(localPosition);// plane.ClosestPoint(localPosition.ToPos4());
            
            Vector2 uv = new Vector2(Vector3.Dot(localPosition, Vector3.UnitX), Vector3.Dot(localPosition, Vector3.UnitY));
            if (uv.X < -1.0 || uv.X > 1.0)
                return null;
            if (uv.Y < -1.0 || uv.Y > 1.0)
                return null;
            uv = XNAExt.Normalize(uv, XNAExt.NegXY, XNAExt.PosXY);
            uv *= Tiling;

            return image.GetPixelBilinear(uv.X, uv.Y);
        }
    }

    public partial class CylinderTextureComponent : BasicTextureComponent
    {
        public override void DebugDraw(DebugRenderer renderer, DebugDrawMode mode)
        {
            renderer.DrawCylinder(Transform, 1.0f, 1.0f, Color.Green);
        }

        public override Color? Sample(Vector3 position, Vector3 normal, SprueBindings.ImageData image)
        {
            if (image == null)
                return null;

            var trans = InverseTransform;
            var localPosition = Vector3.Transform(position, trans);
            var localNormal = Vector3.TransformNormal(normal, trans);

            // Must be inside of the volume
            if (localPosition.Y < -0.5f || localPosition.Y > 0.5f || Mathf.Abs(localPosition.Z) > 0.5f || Mathf.Abs(localPosition.X) > 0.5f)
                return null;

            float u = Mathf.Atan2(localPosition.X, localPosition.Z) / (2 * Mathf.PI) + 0.5f;
            float v = (localPosition.Y * 0.5f) + 0.5f;
            return image.GetPixelBilinear(u * Tiling.X, v * Tiling.Y);
        }
    }

    public class SimpleTextureComponent : TextureComponent
    {
        TextureBlend blend_ = TextureBlend.Overwrite;
        TextureChannel channel_ = TextureChannel.Diffuse;
        TexturePass pass_ = TexturePass.Base;

        [Category("Texture")]
        public TexturePass Pass { get { return pass_; } set { pass_ = value; OnPropertyChanged(); } }
        [Category("Texture")]
        public TextureChannel Channel { get { return channel_; } set { channel_ = value; OnPropertyChanged(); } }
        [Category("Texture")]
        public TextureBlend Blending { get { return blend_; } set { blend_ = value; OnPropertyChanged(); } }
    }

    [PropertyData.PropertyIgnore("Texture")]
    public partial class GradientTextureComponent : SimpleTextureComponent
    {
        bool basedOnNormals_ = false;
        Color lowerColor_ = Color.Black;
        Color upperColor_ = Color.White;

        [Category("Texture")]
        public Color LowerColor { get { return lowerColor_; } set { lowerColor_ = value; OnPropertyChanged(); } }
        [Category("Texture")]
        public Color UpperColor { get { return upperColor_; } set { upperColor_ = value; OnPropertyChanged(); } }
        [Category("Texture")]
        public bool BasedOnNormals { get { return basedOnNormals_; } set { basedOnNormals_ = value;  OnPropertyChanged(); } }

        public override void DebugDraw(DebugRenderer renderer, DebugDrawMode mode)
        {
            renderer.DrawWireBox(Transform, 1, 1, 1, Color.Green);

            BoundingSphere cacheSphere = new BoundingSphere();

            cacheSphere.Radius = 0.25f;
            cacheSphere.Center.X = 0;
            cacheSphere.Center.Y = 1.0f;
            cacheSphere.Center.Z = 0;
            cacheSphere.Center = Vector3.Transform(cacheSphere.Center, Transform);
            renderer.DrawWireSphere(cacheSphere, UpperColor);

            cacheSphere.Center.X = 0;
            cacheSphere.Center.Y = -1.0f;
            cacheSphere.Center.Z = 0;
            cacheSphere.Center = Vector3.Transform(cacheSphere.Center, Transform);
            renderer.DrawWireSphere(cacheSphere, LowerColor);
        }

        public override Color? Sample(Vector3 position, Vector3 normal, SprueBindings.ImageData image)
        {
            var trans = InverseTransform;
            var localPos = Vector3.Transform(position, InverseTransform);
            var localNormal = Vector3.TransformNormal(normal, trans);
            localNormal.Normalize();

            BoundingBox bounds = new BoundingBox(XNAExt.NegXYZ, XNAExt.PosXYZ);
            if (bounds.Contains(localPos) == ContainmentType.Contains)
            {
                if (basedOnNormals_)
                    return XNAExt.Lerp(LowerColor, UpperColor, Mathf.Normalize(localNormal.Y, -1.0f, 1.0f));
                else
                    return XNAExt.Lerp(LowerColor, UpperColor, Mathf.Normalize(localPos.Y, -1.0f, 1.0f));
            }

            return null;
        }
    }

    [PropertyData.PropertyIgnore("Texture")]
    public partial class ColorCubeTextureComponent : SimpleTextureComponent
    {
        Color[] colors_ = { Color.White, Color.White, Color.White, Color.White, Color.White, Color.White };

        [Category("Texture")]
        public Color PositiveXColor { get { return colors_[0]; } set { colors_[0] = value; OnPropertyChanged(); } }
        [Category("Texture")]
        public Color NegativeXColor { get { return colors_[1]; } set { colors_[1] = value; OnPropertyChanged(); } }
        [Category("Texture")]
        public Color PositiveYColor { get { return colors_[2]; } set { colors_[2] = value; OnPropertyChanged(); } }
        [Category("Texture")]
        public Color NegativeYColor { get { return colors_[3]; } set { colors_[3] = value; OnPropertyChanged(); } }
        [Category("Texture")]
        public Color PositiveZColor { get { return colors_[4]; } set { colors_[4] = value; OnPropertyChanged(); } }
        [Category("Texture")]
        public Color NegativeZColor { get { return colors_[5]; } set { colors_[5] = value; OnPropertyChanged(); } }

        public Color index(int idx)
        {
            switch (idx)
            {
                case 0:
                    return PositiveXColor;
                case 1:
                    return NegativeXColor;
                case 2:
                    return PositiveYColor;
                case 3:
                    return NegativeYColor;
                case 4:
                    return PositiveZColor;
                case 5:
                    return NegativeZColor;
            }
            return PositiveXColor;
        }

        static BoundingSphere cacheSphere = new BoundingSphere();
        public override void DebugDraw(DebugRenderer renderer, DebugDrawMode mode)
        {
            renderer.DrawWireBox(Transform, 1.0f, 1.0f, 1.0f, Color.Green);

            cacheSphere.Radius = 0.1f;

            cacheSphere.Center.X = 1.0f;
            cacheSphere.Center.Y = 0;
            cacheSphere.Center.Z = 0;
            cacheSphere.Center = Vector3.Transform(cacheSphere.Center, Transform);
            renderer.DrawWireSphere(cacheSphere, colors_[0]);

            cacheSphere.Center.X = -1.0f;
            cacheSphere.Center.Y = 0;
            cacheSphere.Center.Z = 0;
            cacheSphere.Center = Vector3.Transform(cacheSphere.Center, Transform);
            renderer.DrawWireSphere(cacheSphere, colors_[1]);

            cacheSphere.Center.X = 0;
            cacheSphere.Center.Y = 1.0f;
            cacheSphere.Center.Z = 0;
            cacheSphere.Center = Vector3.Transform(cacheSphere.Center, Transform);
            renderer.DrawWireSphere(cacheSphere, colors_[2]);

            cacheSphere.Center.X = 0;
            cacheSphere.Center.Y = -1.0f;
            cacheSphere.Center.Z = 0;
            cacheSphere.Center = Vector3.Transform(cacheSphere.Center, Transform);
            renderer.DrawWireSphere(cacheSphere, colors_[3]);

            cacheSphere.Center.X = 0;
            cacheSphere.Center.Y = 0;
            cacheSphere.Center.Z = 1.0f;
            cacheSphere.Center = Vector3.Transform(cacheSphere.Center, Transform);
            renderer.DrawWireSphere(cacheSphere, colors_[4]);

            cacheSphere.Center.X = 0;
            cacheSphere.Center.Y = 0;
            cacheSphere.Center.Z = -1.0f;
            cacheSphere.Center = Vector3.Transform(cacheSphere.Center, Transform);
            renderer.DrawWireSphere(cacheSphere, colors_[5]);
        }

        public override Color? Sample(Vector3 position, Vector3 normal, SprueBindings.ImageData image)
        {
            var trans = InverseTransform;
            var localPos = Vector3.Transform(position, trans);
            var localNormal = Vector3.TransformNormal(normal, trans);
            localNormal.Normalize();

            BoundingBox bounds = new BoundingBox(XNAExt.NegXYZ, XNAExt.PosXYZ);
            if (bounds.Contains(localPos) == ContainmentType.Contains)
            {
                var nSquared = (localNormal * localNormal).Abs();
                Color xCol = index(localNormal.X < 0 ? 1 : 0);
                Color yCol = index(localNormal.Y < 0 ? 3 : 2);
                Color zCol = index(localNormal.Z < 0 ? 5 : 4);

                Color xRet = xCol.Mul(nSquared.X);
                Color yRet = yCol.Mul(nSquared.Y);
                Color zRet = zCol.Mul(nSquared.Z);

                return xRet.Add(yRet).Add(zRet);

                //Color ret = index(localNormal.X < 0 ? 1 : 0).Mul(nSquared.X).Add( 
                //    index((localNormal.Y < 0 ? 1 : 0) + 2).Mul(nSquared.Y)).Add(
                //    index((localNormal.Z < 0 ? 1 : 0) + 4).Mul(nSquared.Z));
                //ret.A = 255;
                //return ret;
            }

            return null;
        }
    }

    public partial class DecalStripTextureComponent : TextureComponent
    {
        public override void DebugDraw(DebugRenderer renderer, DebugDrawMode mode)
        {

        }
    }

    public partial class DomeTextureComponent : BasicTextureComponent
    {
        public override void DebugDraw(DebugRenderer renderer, DebugDrawMode mode)
        {
            float minTh = -DataExtensions.M_HALF_PI;
            float maxTh = DataExtensions.M_HALF_PI;
            float minPs = -DataExtensions.M_HALF_PI;
            float maxPs = DataExtensions.M_HALF_PI;
            renderer.DrawSpherePatch(Transform, Vector3.Zero, Vector3.UnitY, Vector3.UnitZ, 1.0f, minTh, maxTh, minPs, maxPs, Color.Green, true);
        }

        public override Color? Sample(Vector3 position, Vector3 normal, SprueBindings.ImageData image)
        {
            if (image == null)
                return null;

            var trans = InverseTransform;
            var localPosition = Vector3.Transform(position, trans);
            var localNormal = Vector3.TransformNormal(normal, trans);
            localNormal.Normalize();

            // must be on the dome side
            if (localPosition.Z < 0)
                return null;
            if (localPosition.Length() > 1.0f)
                return null;

            // This is how the dome works
            localPosition.Normalize();
            //Plane plane = XNAExt.PlaneFromPointNormal(Vector3.Zero, Vector3.UnitZ);
            //Vector3 planePos = plane.Project(localPosition);
            Vector2 uv = new Vector2(Vector3.Dot(localPosition, Vector3.UnitX), Vector3.Dot(localPosition, Vector3.UnitY));
            if (uv.X < -1.0 || uv.X > 1.0)
                return null;
            if (uv.Y < -1.0 || uv.Y > 1.0)
                return null;

            uv = XNAExt.Normalize(uv, XNAExt.NegXY, XNAExt.PosXY);
            uv *= Tiling;
            return image.GetPixelBilinear(uv.X, uv.Y);
        }
    }
}
