using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Color = Microsoft.Xna.Framework.Color;
using Vector2 = Microsoft.Xna.Framework.Vector3;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;
using Quaternion = Microsoft.Xna.Framework.Quaternion;

namespace Zircon
{

    public abstract class MeshElem
    {
        public int Index { get; set; }

        // Editor traits
        public bool IsSelected { get; set; }
    }

    public class Vertex : MeshElem
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Normal = Vector3.Zero;
        public List<int> AdjacentVertices { get; set; } = new List<int>(6);
        public List<int> AdjacentEdges { get; set; } = new List<int>(6);

        internal int nextLevel_;
        internal int subdivID_;
        internal int subdivStep_;
        internal bool isEdgeVert_;

        public Vertex(Vector3 pos) : this(pos, Vector3.Zero)
        {
        }

        public Vertex(Vector3 pos, Vector3 nor)
        {
            Position = pos; Normal = nor;
            isEdgeVert_ = false;
            subdivID_ = 0;
            subdivStep_ = 0;
            nextLevel_ = 0;
        }
    }

    public class Edge : MeshElem
    {
        public int Start { get; set; }
        public int End { get; set; }
        public int Left { get; set; }
        public int Right { get; set; }

        internal int subdivSubdivideId_;
        internal int subdivSubdivideStep_;
        internal int subdivMiddle_;
        internal int subdivStartEdge_;
        internal int subdivEndEdge_;
        internal int subdivRightEdge_;
        internal int subdivLeftEdge_;

        public Edge(int startVert, int endVert)
        {
            Start = startVert;
            End = endVert;
            Left = 0;
            Right = 0;

            subdivSubdivideId_ = 0;
            subdivSubdivideStep_ = 0;
            subdivMiddle_ = 0;
            subdivStartEdge_ = 0;
            subdivEndEdge_ = 0;
            subdivRightEdge_ = 0;
            subdivLeftEdge_ = 0;
        }
    }

    public class Face : MeshElem
    {
        public List<int> Edges { get; set; } = new List<int>();
        public Vector3 Normal { get; set; }
        internal int center_ = 0;

        public int this[int edgeIndex] { get { return Edges[edgeIndex]; } }
    }

    internal class SubdivisionFace : MeshElem
    {
        internal int center_;
        internal int[] edges_ = new int[] { 0, 0, 0, 0 };
        internal int[] subFace_ = new int[] { 0, 0, 0, 0 };
        internal Vector3 normal_;

        public SubdivisionFace(int eA, int eB, int eC, int eD)
        {
            center_ = 0;
            edges_[0] = eA;
            edges_[1] = eB;
            edges_[2] = eC;
            edges_[3] = eD;
        }
    }

    public class SubdivisionLevel
    {
        public IndexArray<Vertex> Vertices { get; set; } = new IndexArray<Vertex>();
        public IndexArray<Edge> Edges { get; set; } = new IndexArray<Edge>();
        public IndexArray<Face> Faces { get; set; } = new IndexArray<Face>();        
    }

    public class Mesh
    {
        public int SubdivisionLevel { get; set; } = 0;
        public int SubdivisionSize { get; set; } = 0;
        public Vector3 Center { get; set; } = Vector3.Zero;
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Quaternion Rotation { get; set; } = Quaternion.Identity;
        public Vector3 Scale { get; set; } = Vector3.One;
        public bool IsHidden { get; set; } = false;
        public bool IsMirrored { get; set; } = false;

        public int VertexCount { get { return vertexArray_.Count; } }
        public int EdgeCount { get { return edgeArray_.Count; } }
        public int FaceCount { get { return faceArray_.Count; } }

        IndexArray<Vertex> vertexArray_ = new IndexArray<Vertex>();
        IndexArray<Edge> edgeArray_ = new IndexArray<Edge>();
        IndexArray<Face> faceArray_ = new IndexArray<Face>();
        List<SubdivisionLevel> subdivisionLevels_ = new List<SubdivisionLevel>();

        public int AddVertex(Vector3 p)
        {
            int idx = vertexArray_.Add(new Vertex(p));
            return idx;
        }

        public int AddVertex(Vector3 p, Vector3 n)
        {
            int idx = vertexArray_.Add(new Vertex(p, n));
            return idx;
        }

        public int AddEdge(int eIdx, Edge e)
        {
            int idx = edgeArray_.AddAt(eIdx, e);
            return idx;
        }

        public int AddFace(int fIdx, Face f)
        {
            int idx = faceArray_.AddAt(fIdx, f);
            return idx;
        }

        public int AddVertex(int vIdx, Vertex v)
        {
            int idx = vertexArray_.AddAt(vIdx, v);
            return idx;
        }

        public int AddEdge(int start, int end)
        {
            int ei = edgeArray_.Add(new Edge(start, end));
            vertexArray_[start].AdjacentEdges.Add(ei);
            vertexArray_[end].AdjacentEdges.Add(ei);
            return ei;
        }

        public Vertex GetVertex(int vIdx)
        {
            return vertexArray_[vIdx];
        }

        public Edge GetEdge(int eIdx)
        {
            return edgeArray_[eIdx];
        }

        public Face GetFace(int fIdx)
        {
            return faceArray_[fIdx];
        }

        public int AddFace(params int[] edgeArray)
        {
            //printf("start\n");
            Face face = new Face();
            int resultIndex = faceArray_.Add(face);
            int size = edgeArray.Length;
            for (int i = 0; i < size; ++i)
            {
                int i1 = edgeArray[i];
                int i2 = edgeArray[(i + 1) % size];
                if (edgeArray_[i1].End == edgeArray_[i2].Start || edgeArray_[i1].End == edgeArray_[i2].End)
                {
                    edgeArray_[i1].Right = face.Index;
                    face.Edges.Add(i1);
                }
                else if (edgeArray_[i1].Start == edgeArray_[i2].Start || edgeArray_[i1].Start == edgeArray_[i2].End)
                {
                    edgeArray_[i1].Left = face.Index;
                    face.Edges.Add(-i1);
                }
            }
            return resultIndex;
        }

        public void Subdivide()
        {
            ++SubdivisionLevel;
            if (SubdivisionSize != 0)
            {
                subdivisionLevels_[0] = new SubdivisionLevel();// vertexArray_.Count + edgeArray_.Count + faceArray_.Count, edgeArray_.Count * 4, edgeArray_.Count * 2);
                ++SubdivisionSize;
                int faceCount = faceArray_.Count;
                for (int i = 1; i < faceCount; ++i)
                {
                    if (faceArray_[i] == null)
                        continue;
                    //TODOSubdivideFace(faceArray_[i]);
                }
            }
            else if (SubdivisionSize > 0 && SubdivisionSize < 5)
            {
                for (int e = SubdivisionSize; e > 0; --e)
                {
                    subdivisionLevels_[e] = subdivisionLevels_[e - 1];
                }
                subdivisionLevels_[0] = new SubdivisionLevel();// m_subdivideLevel[0]->m_vertex.size() + m_subdivideLevel[0]->m_edge.size() + m_subdivideLevel[0]->m_face.size(), m_subdivideLevel[0]->m_edge.size() * 4, m_subdivideLevel[0]->m_edge.size() * 2);
                ++SubdivisionSize;
                int faceCount = subdivisionLevels_[1].Faces.Count;
                for (int i = 1; i < faceCount; i++)
                {
                    if (subdivisionLevels_[1].Faces[i] == null)
                        continue;
                    //TODOSubdivideFace(subdivisionLevels_[1].Faces[i]);
                }
            }
            //TODOUpdateAllSubNormal();
        }

        public Vector3 EAdjacentVertex(Vertex vertex)
        {
            vertex.isEdgeVert_ = false;
            Vector3 result = Vector3.Zero;
            int vertexCount = vertex.AdjacentEdges.Count;
            for (int i = 0; i < vertexCount; ++i)
            {
                if (edgeArray_[vertex.AdjacentEdges[i]].Left != 0 && edgeArray_[vertex.AdjacentEdges[i]].Right != 0 && !vertex.isEdgeVert_)
                {
                    if (vertex.Index == edgeArray_[vertex.AdjacentEdges[i]].End)
                        result += vertexArray_[edgeArray_[vertex.AdjacentEdges[i]].Start].Position;
                    else
                        result += vertexArray_[edgeArray_[vertex.AdjacentEdges[i]].End].Position;
                }
                else
                {
                    if (!vertex.isEdgeVert_)
                    {
                        vertex.isEdgeVert_ = true;
                        //TODO: null?
                        result = Vector3.Zero;
                    }
                    if (edgeArray_[vertex.AdjacentEdges[i]].Left == 0 || edgeArray_[vertex.AdjacentEdges[i]].Right == 0)
                    {
                        if (vertex.Index == edgeArray_[vertex.AdjacentEdges[i]].End)
                            result += vertexArray_[edgeArray_[vertex.AdjacentEdges[i]].Start].Position;
                        else
                            result += vertexArray_[edgeArray_[vertex.AdjacentEdges[i]].End].Position;
                    }
                }
            }
            return result;
        }

        public Vector3 EAdjacentVertex(Vertex vertex, int level)
        {
            vertex.isEdgeVert_ = false;
            Vector3 result = Vector3.Zero;
            int vertexCount = vertex.AdjacentEdges.Count;
            for (int i = 0; i < vertexCount; ++i)
            {
                if (subdivisionLevels_[level].Edges[vertex.AdjacentEdges[i]].Left != 0 && edgeArray_[vertex.AdjacentEdges[i]].Right != 0 && !vertex.isEdgeVert_)
                {
                    if (vertex.Index == subdivisionLevels_[level].Edges[vertex.AdjacentEdges[i]].End)
                        result += subdivisionLevels_[level].Vertices[subdivisionLevels_[level].Edges[vertex.AdjacentEdges[i]].Start].Position;
                    else
                        result += subdivisionLevels_[level].Vertices[subdivisionLevels_[level].Edges[vertex.AdjacentEdges[i]].End].Position;
                }
                else
                {
                    if (!vertex.isEdgeVert_)
                    {
                        vertex.isEdgeVert_ = true;
                        //TODO: null?
                        result = Vector3.Zero;
                    }
                    if (subdivisionLevels_[level].Edges[vertex.AdjacentEdges[i]].Left == 0 || subdivisionLevels_[level].Edges[vertex.AdjacentEdges[i]].Right == 0)
                    {
                        if (vertex.Index == subdivisionLevels_[level].Edges[vertex.AdjacentEdges[i]].End)
                            result += subdivisionLevels_[level].Vertices[subdivisionLevels_[level].Edges[vertex.AdjacentEdges[i]].Start].Position;
                        else
                            result += subdivisionLevels_[level].Vertices[subdivisionLevels_[level].Edges[vertex.AdjacentEdges[i]].End].Position;
                    }
                }
            }
            return result;
        }

        public void NormalizeVertexNormals()
        {
            for (int i = 0; i < vertexArray_.Count; ++i)
                vertexArray_[i].Normal = Vector3.Normalize(vertexArray_[i].Normal);
        }

        public void UpdateFaceNormal(Face face)
        {
            int edgeCount = face.Edges.Count;
            Vector3[] vertexector = new Vector3[edgeCount];
            for (int i = 0; i < edgeCount; ++i)
            {
                vertexector[i] = Vector3.Zero;

                if (face.Edges[i] > 0)
                    vertexector[i] = vertexArray_[edgeArray_[face.Edges[i]].End].Position - vertexArray_[edgeArray_[face.Edges[i]].Start].Position;
                else
                    vertexector[i] = vertexArray_[edgeArray_[-face.Edges[i]].Start].Position - vertexArray_[edgeArray_[-face.Edges[i]].End].Position;
            }
            --edgeCount;
            for (int i = 0; i < edgeCount; ++i)
                face.Normal += Vector3.Cross(vertexector[i], vertexector[i + 1]);
            face.Normal += Vector3.Cross(vertexector[edgeCount], vertexector[0]);
            face.Normal = Vector3.Normalize(face.Normal);
        }

        public void UpdateFaceNormal(Face face, int level)
        {
            Vector3[] vertexector = new Vector3[4];
            for (int i = 0; i < 4; ++i)
            {
                vertexector[i] = Vector3.Zero;
                if (face.Edges[i] > 0)
                    vertexector[i] = subdivisionLevels_[level].Vertices[subdivisionLevels_[level].Edges[face.Edges[i]].End].Position - subdivisionLevels_[level].Vertices[subdivisionLevels_[level].Edges[face.Edges[i]].Start].Position;
                else
                    vertexector[i] = subdivisionLevels_[level].Vertices[subdivisionLevels_[level].Edges[-face.Edges[i]].Start].Position - subdivisionLevels_[level].Vertices[subdivisionLevels_[level].Edges[-face.Edges[i]].End].Position;
            }
            for (int i = 0; i < 3; ++i)
            {
                face.Normal += Vector3.Cross(vertexector[i], vertexector[i + 1]);
            }
            face.Normal += Vector3.Cross(vertexector[3], vertexector[0]);
            face.Normal = Vector3.Normalize(face.Normal);
        }

        public void UpdateAllSubNormal()
        {
            int faceCount = subdivisionLevels_[0].Faces.Count;
            for (int i = 1; i < faceCount; ++i)
            {
                UpdateFaceNormal(subdivisionLevels_[0].Faces[i], 0);
            }
            int vertexCount = subdivisionLevels_[0].Vertices.Count;
            for (int i = 1; i < vertexCount; ++i)
            {
                UpdateVertexNormal(subdivisionLevels_[0].Vertices[i], 0);
            }
        }

        public void UpdateVertexNormal(Vertex vertex)
        {
            int adjCount = vertex.AdjacentEdges.Count;
            vertex.Normal = Vector3.Zero;
            for (int i = 0; i < adjCount; ++i)
            {
                if (edgeArray_[vertex.AdjacentEdges[i]].Start == vertex.Index)
                {
                    Face face = faceArray_[edgeArray_[vertex.AdjacentEdges[i]].Left];
                    if (face != null)
                        vertex.Normal += face.Normal;
                }
                else
                {
                    Face face = faceArray_[edgeArray_[vertex.AdjacentEdges[i]].Right];
                    if (face != null)
                        vertex.Normal += face.Normal;
                }
            }
            vertex.Normal = Vector3.Normalize(vertex.Normal);
        }


        public void UpdateVertexNormal(Vertex vertex, int level)
        {
            int adjCount = vertex.AdjacentEdges.Count;
            vertex.Normal = Vector3.Zero;
            for (int i = 0; i < adjCount; ++i)
            {
                if (subdivisionLevels_[level].Edges[vertex.AdjacentEdges[i]].Start == vertex.Index)
                {
                    Face face = subdivisionLevels_[level].Faces[subdivisionLevels_[level].Edges[vertex.AdjacentEdges[i]].Left];
                    if (face != null)
                        vertex.Normal += face.Normal;
                }
                else
                {
                    Face face = subdivisionLevels_[level].Faces[subdivisionLevels_[level].Edges[vertex.AdjacentEdges[i]].Right];
                    if (face != null)
                        vertex.Normal += face.Normal;
                }
            }
            vertex.Normal = Vector3.Normalize(vertex.Normal);
        }
    }



    public class IndexArray<T> where T : MeshElem
    {
        List<T> array_ = new List<T>();
        List<int> slots_ = new List<int>();

        public IndexArray()
        {
            array_.Add(null);
        }

        public IndexArray(int reserve)
        {
            array_.Add(null);
        }

        public void Clear()
        {
            array_.Clear();
            slots_.Clear();
            array_.Add(null);
        }

        public void PushNull()
        {
            array_.Add(null);
        }

        public void PushNulls()
        {
            slots_.Add(array_.Count);
            array_.Add(null);
        }

        public int AddAt(int ei, T element)
        {
            if (array_[ei] == null)
            {
                for (int e = 0; e < slots_.Count; ++e)
                {
                    if (slots_[e] == ei)
                    {
                        slots_[e] = slots_[0];
                        slots_.RemoveAt(0);
                        array_[ei] = element;
                        return ei;
                    }
                }
                return 0;
            }
            else
            {
                return 0;
            }
        }

        public void DirectPush(T element)
        {
            array_.Add(element);
        }

        public int Add(T element)
        {
            if (slots_.Count == 0)
            {
                int tempIndex = array_.Count;
                array_.Add(element);
                element.Index = tempIndex;
                return tempIndex;
            }
            else
            {
                int tempIndex = slots_[0];
                slots_.RemoveAt(0);
                array_[tempIndex] = element;
                element.Index = tempIndex;
                return tempIndex;
            }
        }

        public void RemoveAt(int index)
        {
            if (index != 0)
            {
                array_[index] = null;
                slots_.Add(index);
            }
            return;
        }

        public T this[int i] { get { return array_[i]; } }

        public int Count { get { return array_.Count; } }
    }
}
