using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data.HalfEdge
{
    [Flags]
    public enum Tags
    {
        None = 0,
        Selected = 1
    }

    public class Vertex
    {
        public int Index { get; set; }
        public Vertex FirstColocal { get; set; }
        public Vertex NextColocal { get; set; }

        public Tags Tags;
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 UV;

        public ColocalVertEnumerable Colocals { get { return new ColocalVertEnumerable(FirstColocal); } }

        public class ColocalVertEnumerable : IEnumerable<Vertex>
        {
            ColocalVertEnumerator enum_;
            public ColocalVertEnumerable(Vertex v) { enum_ = new ColocalVertEnumerator(v); }
            public IEnumerator<Vertex> GetEnumerator() { return enum_; }
            IEnumerator IEnumerable.GetEnumerator() { return enum_; }
        }
        internal class ColocalVertEnumerator : IEnumerator<Vertex>
        {
            Vertex first_;
            Vertex current_;
            public ColocalVertEnumerator(Vertex start) { first_ = start; }
            public Vertex Current { get { return current_; } }
            object IEnumerator.Current { get { return current_; } }
            public void Dispose() { }
            public bool MoveNext()
            {
                if (current_ == null)
                {
                    current_ = first_;
                    return true;
                }
                current_ = current_.NextColocal;
                return current_ != null;
            }

            public void Reset() { current_ = null; }
        }

        #region Colocal functions

        public void AppendColocal(Vertex toWho)
        {
            Vertex cur = toWho;
            while (cur.NextColocal != null)
                cur = cur.NextColocal;
            cur.NextColocal = toWho;
            FirstColocal = cur.FirstColocal;
        }

        public void DetachColocal()
        {
            if (FirstColocal == this)
            {
                var newFirst = FirstColocal.NextColocal;
                foreach (var v in Colocals)
                    v.FirstColocal = newFirst;
                FirstColocal = this;
                NextColocal = null;
            }
            else
            {
                var previous = FirstColocal;
                var next = NextColocal;
                while (next != this)
                {
                    previous = next;
                    next = next.NextColocal;
                }
                if (previous != null && next != null)
                    previous.NextColocal = next.NextColocal;
            }
        }

        /// Moves this vertex along with the colocals
        public void SetPosition(Vector3 newPos)
        {
            foreach (var v in Colocals)
                v.Position = newPos;
        }

        public void SetNormal(Vector3 newNor)
        {
            foreach (var v in Colocals)
                v.Normal = newNor;
        }

        #endregion
    }

    public class Edge
    {
        public Tags Tags;
        public int Index { get; set; }
        public Vertex Start { get; set; }
        public Vertex End { get; set; }
        public Edge Next { get; set; }
        public Edge Prev { get; set; }
        public Edge Pair { get; set; }

        public Edge FirstCoRadial { get; set; }
        public Edge NextCoRadial { get; set; }

        public Face Face { get; set; }

        public bool IsBoundary { get { return Pair == null; } }
        public bool IsSeam {
            get {
                if (IsBoundary) // boundaries are always seams
                    return true;
                if (Pair.Start.UV.Equals(End.UV) && Pair.End.UV.Equals(Start.UV))
                    return false;
                return true;
            }
        }

        public void UpdateIndex(Mesh mesh) { Index = mesh.Edges.IndexOf(this); }

        public void PairWith(Edge rhs)
        {
            Unpair();
            Pair = rhs;
            if (rhs != null)
                rhs.Pair = this;
        }
        public void Unpair()
        {
            if (Pair != null)
                Pair.Pair = null;
        }

        public RadialEdgeEnumerable CoRadial { get { return new RadialEdgeEnumerable(FirstCoRadial); } }

        public class RadialEdgeEnumerable : IEnumerable<Edge>
        {
            RadialEdgeEnumerator enum_;
            public RadialEdgeEnumerable(Edge v) { enum_ = new RadialEdgeEnumerator(v); }
            public IEnumerator<Edge> GetEnumerator() { return enum_; }
            IEnumerator IEnumerable.GetEnumerator() { return enum_; }
        }
        internal class RadialEdgeEnumerator : IEnumerator<Edge>
        {
            Edge first_;
            Edge current_;
            public RadialEdgeEnumerator(Edge start) { first_ = start; }
            public Edge Current { get { return current_; } }
            object IEnumerator.Current { get { return current_; } }
            public void Dispose() { }
            public bool MoveNext()
            {
                if (current_ == null)
                {
                    current_ = first_;
                    return true;
                }
                current_ = current_.NextCoRadial;
                return current_ != null;
            }

            public void Reset() { current_ = null; }
        }
    }

    public class Face
    {
        public class FaceEdgeEnumerable : IEnumerable<Edge>
        {
            FaceEdgeEnumerate enum_;
            public FaceEdgeEnumerable(Face face) { enum_ = new FaceEdgeEnumerate(face); }
            public IEnumerator<Edge> GetEnumerator() { return enum_; }
            IEnumerator IEnumerable.GetEnumerator() { return enum_; }
        }
        public class FaceEdgeEnumerate : IEnumerator<Edge>
        {
            Face face_;
            Edge current_;

            public FaceEdgeEnumerate(Face face)
            {
                face_ = face;
                current_ = face.Edge;
            }

            public Edge Current { get { return current_; } }

            object IEnumerator.Current { get { return current_; } }

            public void Dispose()
            {
                
            }

            public bool MoveNext()
            {
                if (face_.IsLastEdge(current_))
                    return false;
                current_ = current_.Next;
                return true;
            }

            public void Reset()
            {
                current_ = face_.Edge;
            }
        }

        public class FaceVertexEnumerable : IEnumerable<Vertex>
        {
            FaceVertexEnumerate enum_;
            public FaceVertexEnumerable(Face face) { enum_ = new FaceVertexEnumerate(face); }
            public IEnumerator<Vertex> GetEnumerator() { return enum_; }
            IEnumerator IEnumerable.GetEnumerator() { return enum_; }
        }
        public class FaceVertexEnumerate : IEnumerator<Vertex>
        {
            Face face_;
            Edge current_;

            public FaceVertexEnumerate(Face face)
            {
                face_ = face;
                current_ = face.Edge;
            }

            public Vertex Current { get { return current_.Start; } }

            object IEnumerator.Current { get { return current_.Start; } }

            public void Dispose() { }

            public bool MoveNext()
            {
                if (face_.IsLastEdge(current_))
                    return false;
                current_ = current_.Next;
                return true;
            }

            public void Reset()
            {
                current_ = face_.Edge;
            }
        }

        public Tags Tags;
        public int Index { get; set; }
        public Edge Edge { get; set; }
        public bool DoubleSided { get; set; } = false;

        /// <summary>
        /// Returns true if the given edge is the last edge
        /// </summary>
        public bool IsLastEdge(Edge e) { return e.Next == Edge; }

        public void UpdateIndex(Mesh mesh) { Index = mesh.Faces.IndexOf(this); }

        public FaceEdgeEnumerable Edges { get { return new FaceEdgeEnumerable(this); } }

        public FaceVertexEnumerable Vertices { get { return new FaceVertexEnumerable(this); } }

        public Edge GetEdgeStartingAt(Vertex v)
        {
            foreach (var edge in Edges)
                if (edge.Start == v)
                    return edge;
            return null;
        }

        public Edge GetEdgeEndingAt(Vertex v)
        {
            foreach (var edge in Edges)
            {
                if (edge.End == v)
                    return edge;
            }
            return null;
        }

        public void ReplaceEdge(Edge who, Edge with)
        {
            if (Edge == who)
            {
                with.Next = Edge.Next;
                with.Prev = Edge.Prev;
                with.Pair = Edge.Pair;
                with.Face = this;
                return;
            }
        }
    }

    public class Mesh
    {
        public Dictionary<Vector3, Vertex> colocalVertices { get; private set; } = new Dictionary<Vector3, Vertex>();
        public List<Vertex> Vertices { get; set; }
        public List<Edge> Edges { get; set; }
        public List<Face> Faces { get; set; }

        public void AddFace(Tags wTags, params int[] vertexIndices)
        {
            var verts = vertexIndices.Select(i => Vertices[i]);
        }

        public void DeleteFace(Face f, bool autoUpdate = true)
        {
            Faces.Remove(f);
            var edges = Edges.Where(e => e.Face == f);
            foreach (var edge in edges)
            {
                if (edge.Pair != null)
                    edge.Pair.Pair = null;
                Edges.Remove(edge);
            }
            if (autoUpdate)
                UpdateEdgeIndices();
        }

        public void DeleteEdge(Edge e, bool autoUpdate = true)
        {
            DeleteFace(e.Face, false);
            if (e.Pair != null)
                DeleteFace(e.Pair.Face, false);
            if (autoUpdate)
                UpdateEdgeIndices();
        }

        public void CollapseEdge(Edge e)
        {
            Vertex fromVert = e.Start;
            Vertex destVert = e.End;

            Face f = e.Face;
            Face of = e.Pair != null ? e.Pair.Face : null;
            Edge pair = e.Pair;

            JoinEdgesOnto(e.Next.Pair, e.Prev.Pair, destVert);
            if (pair != null)
                JoinEdgesOnto(pair.Prev.Pair, pair.Next.Pair, destVert);

            Vertices.Remove(fromVert);
            Edges.Remove(e);
            Edges.Remove(e.Pair);
            Faces.Remove(f);
            Faces.Remove(of);
            UpdateAllIndices();
        }

        class SplitResult
        {
            public Vertex NewVert;
            public Edge left;
            public Edge right;
        }
        SplitResult SplitEdge_Internal(Edge e, float dist, bool firstSide, Vertex existing, bool seam)
        {
            // Create a new vertex if a second call and on a seam
            bool makeNewVertex = firstSide || seam;
            Vertex newVertex = makeNewVertex ? new Vertex() : existing;
            Vertex oldStart = e.Start;
            Edge left = e;
            Edge right = new Edge();
            Face leftFace = e.Face;

            // Setup the left-side triangle
            Edge originalPrevPair = left.Prev.Pair;
            left.Start = newVertex;
            left.Prev.End = newVertex;
            left.Prev.Unpair();

            // build right side triangle
            Face rightFace = new Face { Edge = right, Index = Faces.Count };
            right.Start = oldStart;
            right.End = newVertex;
            right.Next = new Edge { Start = newVertex, End = left.Prev.Start, Face = rightFace };
            right.Prev = new Edge { End = oldStart, Start = right.Next.End, Face = rightFace };
            right.Prev.PairWith(originalPrevPair);
            right.Next.PairWith(left.Prev);

            Edges.Add(right);
            Edges.Add(right.Next);
            Edges.Add(right.Prev);
            Faces.Add(rightFace);

            right.UpdateIndex(this);
            right.Next.UpdateIndex(this);
            right.Prev.UpdateIndex(this);
            rightFace.UpdateIndex(this);
            if (newVertex != existing)
            {
                Vertices.Add(newVertex);
                newVertex.Index = Vertices.Count - 1;
            }

            return new HalfEdge.Mesh.SplitResult {
                NewVert = newVertex,
                left = left,
                right = right
            };
        }

        public void SplitEdge(Edge e, float dist = 0.5f)
        {
            Edge pairedEdge = e.Pair;
            bool seam = e.IsSeam;
            SplitResult leftResult = SplitEdge_Internal(e, dist, true, null, seam);
            SplitResult rightResult = pairedEdge != null ? SplitEdge_Internal(pairedEdge, 1.0f - dist, false, leftResult.NewVert, seam) : null;

            if (rightResult != null)
            {
                leftResult.left.PairWith(rightResult.right);
                leftResult.right.PairWith(rightResult.left);
            }
        }

        void JoinEdgesOnto(Edge a, Edge b, Vertex v)
        {
            if (a != null)
            {
                a.Start = v;
                a.Pair = b;
            }
            if (b != null)
            {
                b.End = v;
                b.Pair = a;
            }
        }

        public void FlipEdge(Edge e)
        {
            if (e.Pair != null)
            {

            }
        }

        void UpdateEdgeIndices()
        {
            int i = 0;
            foreach (var edge in Edges)
            {
                edge.Index = i;
                ++i;
            }
        }
        void UpdateFaceIndicies()
        {
            int idx = 0;
            foreach (var face in Faces)
            {
                face.Index = idx;
                ++idx;
            }
        }
        void UpdateAllIndices() { UpdateEdgeIndices(); UpdateFaceIndicies(); }
    }

    public class EulerOperators
    {
        public static List<Vertex> SplitEdgeMakeVertex(Mesh m, Edge edge)
        {
            List<Vertex> newVerts = new List<Vertex>();
            foreach (var colocalEdge in edge.CoRadial)
            {
                
            }
            return newVerts;
        }

        public static Edge JoinEdgeKillVert(Mesh m, Edge edge, Vertex onto)
        {
            return null;
        }

        public static Face JoinFaceKillEdge(Mesh m, Face face, Edge edge)
        {
            return null;
        }

        public static bool SplitFaceMakeEdge(Mesh m, Vertex a, Vertex b)
        {
            return false;
        }
    }
}
