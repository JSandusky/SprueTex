using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SprueKit.Data.ShapeGen
{
    [Flags]
    public enum PartitionAxis
    {
        None = 0,
        X = 1,
        Y = 2,
        Z = 4,
        All = 1 & 2 & 4
    }

    

    public class PartitionRule
    {
        public PartitionRule Parent { get; set; }
        public Matrix Transform { get; set; }
        public BoundingBox Bounds { get; set; }
        public bool Occlude { get; set; } = false;

        public virtual void Preprocess(VolumePartitioner volume)
        {
        }

        public virtual void Execute(VolumePartitioner volume)
        {

        }

        public virtual bool Parse(JValue jsonValue)
        {
            if (jsonValue.Type == JTokenType.Object)
            {

            }
            return false;
        }

        protected void ParseChildren(JObject self)
        {
            if (self.Value<string>("rule") != null)
            {

            }
            else if (self.Value<JArray>("rules") != null)
            {

            }
        }
    }

    public class AxialRule : PartitionRule
    {
        public PartitionAxis Axes { get; set; } = PartitionAxis.All;
    }

    /// <summary>
    /// Subdivides the current volume along axes
    /// </summary>
    public class SubdivideRule : AxialRule
    {
        public float[] Sizes { get; set; }

        public override void Preprocess(VolumePartitioner volume)
        {
            base.Preprocess(volume);
        }

        public override void Execute(VolumePartitioner volume)
        {
            base.Execute(volume);
        }
    }

    /// <summary>
    /// Creates a nine-slice for the XZ axis
    /// </summary>
    public class NinesliceRule : AxialRule
    {
        public float EdgeSize { get; set; } = 1.0f;

        BoundingBox[,,] Partition(BoundingBox bounds)
        {
            var doubleEdgeSize = EdgeSize * 2;
            var volumeSize = bounds.Size();
            Vector3 centerSize = new Vector3(volumeSize.X - doubleEdgeSize, volumeSize.Y - doubleEdgeSize, volumeSize.Z - doubleEdgeSize);

            int xParts = Axes.HasFlag(PartitionAxis.X) ? 1 : 3;
            int yParts = Axes.HasFlag(PartitionAxis.Y) ? 1 : 3;
            int zParts = Axes.HasFlag(PartitionAxis.Z) ? 1 : 3;

            float[,] starts = new float[,]
                {
                    { 0, EdgeSize, volumeSize.X - EdgeSize },
                    { 0, EdgeSize, volumeSize.Y - EdgeSize },
                    { 0, EdgeSize, volumeSize.Z - EdgeSize },
                };
            float[,] ends = new float[,]
            {
                    { EdgeSize, volumeSize.X - EdgeSize, EdgeSize },
                    { EdgeSize, volumeSize.Y - EdgeSize, EdgeSize },
                    { EdgeSize, volumeSize.Z - EdgeSize, EdgeSize }
            };

            BoundingBox[,,] children = new BoundingBox[9, 9, 9];
            for (int x = 0; x < xParts; ++x)
                for (int y = 0; y < yParts; ++y)
                    for (int z = 0; z < zParts; ++z)
                    {
                        Vector3 cellStart = new Vector3(starts[x, z], starts[y, z], starts[z, z]);
                        Vector3 cellEnd = new Vector3(ends[x, z], ends[y, z], ends[z, z]);

                        children[x, y, z] = new BoundingBox(bounds.Min + cellStart, bounds.Min + cellStart + cellEnd);
                    }

            return children;
        }

        public override void Preprocess(VolumePartitioner volume)
        {
            BoundingBox[,,] cells = Partition(volume.Bounds);

            foreach (var item in cells)
                volume.Children.Add(new VolumePartitioner(item, null, volume, null, volume.OcclusionAreas));
        }
    }

    public class RepeatRule : AxialRule
    {

    }

    /// <summary>
    /// Places a mesh into the current cell
    /// </summary>
    public class MeshRule : PartitionRule
    {

    }

    public class ExtrudePolygonRule : PartitionRule
    {

    }

    public class RevolvePolylineRule : PartitionRule
    {

    }

    public class OccludeRule : PartitionRule
    {
    }

    /// <summary>
    /// Resizes the current cell
    /// </summary>
    public class ResizeRule : PartitionRule
    {

    }

    /// <summary>
    /// Offsets the current cell
    /// </summary>
    public class OffsetRule : PartitionRule
    {

    }

    /// <summary>
    /// Checks for occlusion, performs the appropriate rule otherwise
    /// </summary>
    public class OcclusionTestRule : PartitionRule
    {

    }

    /// <summary>
    /// Performs a test
    /// </summary>
    public class ConditionalRule : PartitionRule
    {

    }

    /// <summary>
    /// Sets a system variable in the stack
    /// </summary>
    public class SetVariableRule : PartitionRule
    {
    }

    public class VolumePartitioner
    {
        public PartitionRule Rule { get; set; }

        public Dictionary<string, JValue> Methods { get; private set; } = new Dictionary<string, JValue>();
        public Dictionary<string, JValue> TemporaryMethods { get; private set; } = new Dictionary<string, JValue>();
        public Dictionary<string, string> Definitions { get; private set; } = new Dictionary<string, string>();
        public Dictionary<string, float> Variables { get; private set; } = new Dictionary<string, float>();

        public VolumePartitioner Parent { get; set; }
        public List<Occlusion> OcclusionAreas { get; private set; } = new List<Occlusion>();
        public List<VolumePartitioner> Children { get; set; } = new List<VolumePartitioner>();

        public Matrix ParentTransform { get; set; } = Matrix.Identity;
        public Matrix LocalTransform { get; set; } = Matrix.Identity;
        public Matrix FinalTransform { get; set; } = Matrix.Identity;
        public Matrix InvFinalTransform { get; set; } = Matrix.Identity;

        public Vector3 ParentPosition { get; set; }
        public Vector3 Snap { get; set; }
        public BoundingBox Bounds { get; set; }

        public Vector3 Size { get { return Bounds.Max - Bounds.Min; } }

        public VolumePartitioner(BoundingBox bounds, JValue jsonValue, VolumePartitioner parent, Dictionary<string, JValue> methods, List<Occlusion> occlusionAreas)
        {
            Bounds = bounds;
            Methods = methods;
            OcclusionAreas = occlusionAreas;

            if (parent != null)
            {
                // Copy methods
                foreach (var method in parent.TemporaryMethods)
                    TemporaryMethods[method.Key] = method.Value;

                // Copy definitions and variables
                foreach (var def in parent.Definitions)
                    Definitions[def.Key] = def.Value;

                foreach (var variable in parent.Variables)
                    Variables[variable.Key] = variable.Value;
            }
        }

        public void Transform(Matrix transform)
        {
            LocalTransform = transform * LocalTransform;
            FinalTransform = ParentTransform * LocalTransform;
            InvFinalTransform = Matrix.Invert(FinalTransform);
        }

        public void Preprocess()
        {
            if (Rule != null)
                Rule.Preprocess(this);

            foreach (var child in Children)
                child.Preprocess();
        }

        public void Execute()
        {
            if (Rule != null)
                Rule.Execute(this);

            foreach (var child in Children)
                child.Execute();
        }
    }
}
