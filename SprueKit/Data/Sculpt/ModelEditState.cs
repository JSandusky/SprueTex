using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data.Sculpt
{
    public abstract class ModelEditState
    {
        /// <summary>
        /// Implementation should return true if the state is valid (ie. has good indices)
        /// </summary>
        public abstract bool IsValid();

        /// <summary>
        /// Implementation returns a suitable transform matrix for making edits (ie. centroid relative)
        /// </summary>
        public abstract Matrix GetTransform();

        /// <summary>
        /// Implementation applies a transform matrix (likely relative to a centroid or such)
        /// </summary>
        public abstract void SetTransform(Matrix transform);
    }

    /// <summary>
    /// State responsible for editing a collection of vertices
    /// </summary>
    public class VertexEditState
    {
        public List<int> Indices { get; set; } = new List<int>();
    }

    /// <summary>
    /// State responsible for editing a collection of edges
    /// </summary>
    public class EdgeEditState
    {
        public List<int> Edges { get; set; } = new List<int>();
    }

    /// <summary>
    /// State responsible for editing a collection of faces
    /// </summary>
    public class FaceEditState
    {
        public List<int> StartIndices { get; set; } = new List<int>();
    }
}
