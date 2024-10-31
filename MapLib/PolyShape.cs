using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vector2 = Microsoft.Xna.Framework.Vector2;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace MapLib
{
    struct RadialRange
    {
        public float Min;
        public float Max;

        public bool Contained(float degrees)
        {
            return IsAngleBetween(degrees, Min, Max);
        }

        public static bool IsAngleBetween(float angle, float a, float b)
        {
            angle = (360 + (angle % 360)) % 360;
            a = (3600000 + a) % 360;
            b = (3600000 + b) % 360;

            if (a < b)
                return a <= angle && angle <= b;
            return a <= angle || angle <= b;
        }

        public void FlipRange()
        {
            var m = Min;
            Min = Max;
            Max = Min;
        }
    }

    public class PolyShape : MapObject
    {
        bool isPolyline_ = false;

        public Polygon Poly { get; set; }
        public override Vector2 Centroid { get { return Vector2.Zero; } }
        public List<PolyShapeBorderLayer> BorderLayers { get; private set; } = new List<PolyShapeBorderLayer>();

        /// <summary>
        /// If true then the last segment of the polygon will be ignored
        /// </summary>
        public bool IsPolyline { get { return isPolyline_; } set { isPolyline_ = value; } }

        public void BuildBorders()
        {
            for (int i = 0; i < BorderLayers.Count; ++i)
                BorderLayers[i].Build();
        }
    }

    public abstract class PolyShapeBorderLayer
    {
        /// <summary>
        /// Indicates that this border is an interior placement such as a shadow
        /// </summary>
        public bool IsInterior { get; set; } = false;

        public abstract void Build();
    }

    /// <summary>
    /// Generates a quad-strip along the border
    /// </summary>
    public class PolyShapeQuadBorderLayer : PolyShapeBorderLayer
    {
        public override void Build()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Places MapObject instances along the border
    /// </summary>
    public class PolyShapeObjectBorderLayer : PolyShapeBorderLayer
    {
        public override void Build()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Uses more intelligent handling of ends and alternating segments
    /// </summary>
    public class PolyShapeSmartQuadBorderLayer : PolyShapeBorderLayer
    {
        public override void Build()
        {
            throw new NotImplementedException();
        }
    }
}
