using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace SprueKit.Data.MeshEditor
{
    public class MInternalVertex
    {
        public Vector3 m_Pos;
        public Vector3 m_Normal;
        public Color vertexColour;
        public byte flags;

        public bool IsFlagged(byte flags) { return (this.flags & flags) != 0; }
        public void SetFlag(byte flags) { this.flags |= flags; }
        public void UnsetFlag(byte flags) { this.flags = (byte)(this.flags & ~flags); }
    }

    public class MTriangle
    {
        public byte[] edgeFlags = new byte[] { 0, 0, 0 };
        public int[] m_Vertices = new int[] { -1, -1, -1 };
        public byte flags = 0;

        public Vector3 m_Normal, m_Centre, colour;

        public MTriangle()
        {
            m_Vertices[0] = -1;
            m_Vertices[1] = -1;
            m_Vertices[2] = -1;
            edgeFlags[0] = 0;
            edgeFlags[1] = 0;
            edgeFlags[2] = 0;
        }

        public MTriangle(int a, int b, int c)
        {
            m_Vertices[0] = a;
            m_Vertices[1] = b;
            m_Vertices[2] = c;
            edgeFlags[0] = 0;
            edgeFlags[1] = 0;
            edgeFlags[2] = 0;
        }

        public MTriangle(MTriangle src)
        {
            m_Vertices[0] = src.m_Vertices[0];
            m_Vertices[1] = src.m_Vertices[1];
            m_Vertices[2] = src.m_Vertices[2];
            edgeFlags[0] = src.edgeFlags[0];
            edgeFlags[1] = src.edgeFlags[1];
            edgeFlags[2] = src.edgeFlags[2];
            flags = src.flags;
        }

        public int GetEdge(int VertA, int VertB)
        {
            if (VertA == m_Vertices[0] && VertB == m_Vertices[1])
                return 0;
            if (VertB == m_Vertices[0] && VertA == m_Vertices[1])
                return 0;
            if (VertA == m_Vertices[1] && VertB == m_Vertices[2])
                return 1;
            if (VertB == m_Vertices[1] && VertA == m_Vertices[2])
                return 1;
            if (VertA == m_Vertices[2] && VertB == m_Vertices[0])
                return 2;
            if (VertB == m_Vertices[2] && VertA == m_Vertices[0])
                return 2;
            return -1;
        }

        public bool IsFlagged(byte val) { return (flags & val) != 0; }
    }

    public class MeshBase
    {
        public static readonly byte VERTEX_NORMAL_OKAY = 1;
        public static readonly byte VERTEX_NORMAL_NEEDS_UPDATE = 2;

        public int NumVerts { get { return m_Verts.Count; } }
        public int NumTris { get { return m_Tris.Count; } }

        public Vector3 GetVertexPosition(int index)
        {
            if (index < m_Verts.Count)
                return m_Verts[index].m_Pos;
            return Vector3.Zero;
        }
        public Vector3 GetVertexNormal(int index)
        {
            if (index < m_Verts.Count)
                return m_Verts[index].m_Normal;
            return Vector3.Zero;
        }

        #region Vertex flagging
        public void SetVertexFlag(int index, byte flag)
        {
            if (index < m_Verts.Count)
                m_Verts[index].flags = m_Verts[index].flags.SetFlag(flag);
        }

        public void UnsetVertexFlag(int index, byte flag) {
            if (index < m_Verts.Count)
                m_Verts[index].flags = m_Verts[index].flags.UnsetFlag(flag);
        }

        public bool IsVertexFlagged(int index, byte flag)
        {
            if (index < m_Verts.Count)
                return m_Verts[index].flags.HasFlag(flag);
            return false;
        }
        #endregion

        #region Triangles

        public MTriangle GetTri(int idx)
        {
            if (idx < m_Tris.Count)
                return m_Tris[idx];
            return null;
        }
        public int GetTriVert(int TriNum, int VertNum)
        {
            if (TriNum < 0 || TriNum >= NumTris)
                return -1;
            return m_Tris[TriNum].m_Vertices[VertNum];
        }
        public bool GetTriFromEdge(int EdgeNum, out MTriangle Tri, out int Edge)
        {
            Tri = null;
            Edge = -1;

            int TriNum;
            TriNum = EdgeNum / 3;
            Tri = GetTri(TriNum);

            if (Tri == null)
                return false;
            Edge = EdgeNum % 3;
            return true;
        }
        public bool GetVertsFromEdge(int EdgeNum, out int VertA, out int VertB)
        {
            VertA = 0;
            VertB = 0;
            int TriNum, TriEdge;
            TriNum = EdgeNum / 3;
            TriEdge = EdgeNum % 3;
            if (TriNum >= NumTris)
                return false;

            if (TriNum < 0)
                return false;

            if (TriEdge == 0)
            {
                VertA = m_Tris[TriNum].m_Vertices[0];
                VertB = m_Tris[TriNum].m_Vertices[1];
            }
            else if (TriEdge == 1)
            {
                VertA = m_Tris[TriNum].m_Vertices[1];
                VertB = m_Tris[TriNum].m_Vertices[2];
            }
            else if (TriEdge == 2)
            {
                VertA = m_Tris[TriNum].m_Vertices[2];
                VertB = m_Tris[TriNum].m_Vertices[0];
            }
            return true;
        }
        public int GetTriangleNeighbour(int triIndex, int edgeIndex)
        {
            int vertA = m_Tris[triIndex].m_Vertices[edgeIndex];
            int vertB = m_Tris[triIndex].m_Vertices[(edgeIndex + 1) % 3];
            for (int triNum = 0; triNum < NumTris; ++triNum)
            {
                if (triNum != triIndex && m_Tris[triNum].GetEdge(vertA, vertB) != -1)
                    return triNum;
            }
            return -1;
        }

        public int GetNumVertsFlagged(byte flags)
        {
            int ct = 0;
            foreach (var vert in m_Verts)
                if (vert.flags.HasFlag(flags))
                    ++ct;
            return ct;
        }
        public int GetNumTrisFlagged(byte flags)
        {
            int ct = 0;
            foreach (var tri in m_Tris)
                if (tri.flags.HasFlag(flags))
                    ++ct;
            return ct;
        }

        #endregion

        public BoundingBox GetMeshExtents()
        {
            Vector3 minVec = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 maxVec = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            foreach (var v in m_Verts)
            {
                minVec = Vector3.Min(minVec, v.m_Pos);
                maxVec = Vector3.Max(maxVec, v.m_Pos);
            }
            return new BoundingBox(minVec, maxVec);
        }

        #region Bulk Flags

        public void FlagVerticesOnTris(byte TriFlags, byte VertFlags)
        {
            foreach (var tri in m_Tris)
            {
                if (tri.flags.HasFlag(TriFlags))
                {
                    m_Verts[tri.m_Vertices[0]].flags = m_Verts[tri.m_Vertices[0]].flags.SetFlag(VertFlags);
                    m_Verts[tri.m_Vertices[1]].flags = m_Verts[tri.m_Vertices[1]].flags.SetFlag(VertFlags);
                    m_Verts[tri.m_Vertices[2]].flags = m_Verts[tri.m_Vertices[2]].flags.SetFlag(VertFlags);
                }
            }
        }

        public void SetVertexFlags(byte Flags) { foreach (var v in m_Verts) v.flags = v.flags.SetFlag(Flags); }
        public void UnsetVertexFlags(byte Flags) { foreach (var v in m_Verts) v.flags = v.flags.UnsetFlag(Flags); }
        public void SetTriangleFlags(byte Flags) { foreach (var t in m_Tris) t.flags = t.flags.SetFlag(Flags); }
        public void UnsetTriangleFlags(byte Flags) { foreach (var t in m_Tris) t.flags = t.flags.UnsetFlag(Flags); }

        #endregion

        #region Protected members
        protected List<MInternalVertex> m_Verts = new List<MInternalVertex>();
        protected List<MTriangle> m_Tris = new List<MTriangle>();
        #endregion


        public void InvalidateNormals() { normalsInvalid = true; }

        public void CalculateNormals()
        {
            if (!normalsInvalid)
                return;

            if (m_Verts.Count == 0)
                return;

            for (int n = 0; n < NumVerts; n++)
            {
                if (!m_Verts[n].IsFlagged(VERTEX_NORMAL_OKAY))
                    m_Verts[n].m_Normal = Vector3.Zero;
                else
                    m_Verts[n].UnsetFlag(VERTEX_NORMAL_NEEDS_UPDATE);
            }

            Vector3 va, vb, vc, vd, ve, vn;
            for (int n = 0; n < NumTris; n++)
            {
                var T = m_Tris[n];

                MInternalVertex[] v = new MInternalVertex[] {
                    m_Verts[T.m_Vertices[0]],
                    m_Verts[T.m_Vertices[1]],
                    m_Verts[T.m_Vertices[2]]
                };

                if (v[0].IsFlagged(VERTEX_NORMAL_OKAY) &&
                    v[1].IsFlagged(VERTEX_NORMAL_OKAY) &&
                    v[2].IsFlagged(VERTEX_NORMAL_OKAY))
                {
                    continue;
                }

                va = new Vector3(v[0].m_Pos.X, v[0].m_Pos.Y, v[0].m_Pos.Z);
                vb = new Vector3(v[1].m_Pos.X, v[1].m_Pos.Y, v[1].m_Pos.Z);
                vc = new Vector3(v[2].m_Pos.X, v[2].m_Pos.Y, v[2].m_Pos.Z);

                vd = vb - va;
                ve = vc - vb;

                T.m_Normal = vd / ve;
                T.m_Centre = (va + vb + vc) * (float)(1.0 / 3.0);

                v[0].SetFlag(VERTEX_NORMAL_NEEDS_UPDATE);
                v[1].SetFlag(VERTEX_NORMAL_NEEDS_UPDATE);
                v[2].SetFlag(VERTEX_NORMAL_NEEDS_UPDATE);
                v[0].m_Normal = Vector3.Zero;
                v[1].m_Normal = Vector3.Zero;
                v[2].m_Normal = Vector3.Zero;

            }

            for (int n = 0; n < NumTris; n++)
            {
                var T = m_Tris[n];
                MInternalVertex[] v = new MInternalVertex[] {
                    m_Verts[T.m_Vertices[0]],
                    m_Verts[T.m_Vertices[1]],
                    m_Verts[T.m_Vertices[2]]
                };

                if (v[0].IsFlagged(VERTEX_NORMAL_NEEDS_UPDATE))
                    v[0].m_Normal += T.m_Normal;
                if (v[1].IsFlagged(VERTEX_NORMAL_NEEDS_UPDATE))
                    v[1].m_Normal += T.m_Normal;
                if (v[2].IsFlagged(VERTEX_NORMAL_NEEDS_UPDATE))
                    v[2].m_Normal += T.m_Normal;

                // do the triangle normalise *after* altering the normals of 
                // the vertices, so the area of the triangle effects the outcome.
                T.m_Normal = Vector3.Normalize(T.m_Normal);
            }

            for (int n = 0; n < NumVerts; n++)
            {
                if (m_Verts[n].IsFlagged(VERTEX_NORMAL_NEEDS_UPDATE))
                {
                    m_Verts[n].m_Normal = Vector3.Normalize(m_Verts[n].m_Normal);
                    m_Verts[n].SetFlag(VERTEX_NORMAL_OKAY);
                }
            }

            normalsInvalid = false;
        }
        bool normalsInvalid = true;
    }
}
