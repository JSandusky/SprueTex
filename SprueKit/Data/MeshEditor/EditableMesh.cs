using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;
using Color = Microsoft.Xna.Framework.Color;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace SprueKit.Data.MeshEditor
{
    public class EditableMesh : MeshBase
    {

        static readonly byte VERTEX_FLAGFORCHANGE = 1;
        static readonly byte EDGE_VISIBLE = 2;
        static readonly byte VERTEX_VISIBLE = 4;
        static readonly byte VERTEX_SELECTED = 8;

        #region Vertex functions
        public virtual int AddVertex(float x, float y, float z)
        {
            m_Verts.Add(new MInternalVertex { m_Pos = new Vector3(x, y, z) });
            return m_Verts.Count - 1;
        }
        public virtual int AddVertexArray(params Vector3[] verts)
        {
            foreach (var v in verts)
                m_Verts.Add(new MInternalVertex { m_Pos = v });
            return m_Verts.Count - verts.Length;
        }
        public virtual void DeleteVertex(int idx)
        {
            m_Verts.RemoveAt(idx);
        }

        public virtual void DeleteVertexArray(params int[] indices)
        {
            if (indices == null || indices.Length == 0)
                return;
            MInternalVertex[] toRemove = new MInternalVertex[indices.Length];
            int i = 0;
            foreach (var idx in indices)
            {
                toRemove[i] = m_Verts[idx];
                ++i;
            }

            foreach (var v in toRemove)
                m_Verts.Remove(v);
        }
        public void DeleteVertexFlag(byte Flag)
        {
            for (int i = 0; i < m_Verts.Count; ++i)
            {
                if (m_Verts[i].flags.HasFlag(Flag))
                {
                    m_Verts.Remove(m_Verts[i]);
                    --i;
                }
            }
        }
        #endregion

        #region Triangle functions

        public virtual void AddTriangle(int A, int B, int C)
        {
            m_Tris.Add(new MeshEditor.MTriangle(A, B, C));
        }
        public virtual void AddTriangle(Vector3 A, Vector3 B, Vector3 C)
        {
            AddVertexArray(A, B, C);
            AddTriangle(NumVerts - 3, NumVerts - 2, NumVerts - 1);
        }
        public virtual void AddTriangleArray(params MTriangle[] tris)
        {
            m_Tris.AddRange(tris);
        }
        public virtual void DeleteTriangle(int idx)
        {
            m_Tris.RemoveAt(idx);
        }
        public virtual void DeleteTriangleArray(params int[] indices)
        {
            if (indices == null || indices.Length == 0)
                return;
            MTriangle[] toRemove = new MTriangle[indices.Length];
            for (int i = 0; i < indices.Length; ++i)
                toRemove[i] = m_Tris[indices[i]];
            foreach (var tri in toRemove)
                m_Tris.Remove(tri);
        }
        public void DeleteTriangleFlag(byte flag)
        {
            for (int i = 0; i < m_Tris.Count; ++i)
            {
                if (m_Tris[i].flags.HasFlag(flag))
                {
                    m_Tris.RemoveAt(i);
                    --i;
                }
            }
        }

        public void AddVertsAndTriangles(Vector3[] Verts, MTriangle[] Tris)
        {
            // move 0 based indices forward
            int adjust = NumVerts;
            for (int n = 0; n < Tris.Length; ++n)
            {
                Tris[n].m_Vertices[0] += adjust;
                Tris[n].m_Vertices[1] += adjust;
                Tris[n].m_Vertices[2] += adjust;
            }

            AddVertexArray(Verts);
            AddTriangleArray(Tris);
        }

        #endregion

        #region Basic Editing

        public void SetVertexPosition(int index, Vector3 pos)
        {
            if (index < 0 || index > NumVerts)
                return;
            m_Verts[index].m_Pos = pos;
        }
        public void SetVertexNormal(int index, Vector3 normal)
        {
            if (index < 0 || index > NumVerts)
                return;
            m_Verts[index].m_Normal = normal;
        }

        public virtual bool TurnEdge(int EdgeNum)
        {
            int TriNumA, TriNumB, EdgeA, EdgeB = 0, VertA, VertB, n;

            TriNumA = EdgeNum / 3;
            EdgeA = EdgeNum % 3;

            MTriangle TriA, TriB;
            TriA = m_Tris[TriNumA];

            if (TriA == null)
                return false;

            // We now have one edge which we need to turn.
            VertA = TriA.m_Vertices[EdgeA];
            VertB = TriA.m_Vertices[(EdgeA + 1) % 3];

            TriB = null;
            // Now find the other edge of the triangle we need.
            {
                MTriangle pTri = null;

                TriNumB = 0;
                for (n = NumTris - 1; n >= 0; n--)
                {
                    pTri = m_Tris[n];
                    EdgeB = pTri.GetEdge(VertA, VertB);

                    if (EdgeB != -1 && TriNumB != TriNumA)
                    {
                        TriB = pTri;
                        break;
                    }

                    TriNumB++;
                }
            }

            // we can't turn an edge if there isn't two triangles.
            if (TriB == null)
                return false;

            // now know both triangles, and both edge numbers. just have to turn them now
            // generate a quad, with the edge in question across the diagonal.
            // 0----1          3----0
            // |   /|          |   /| 
            // |  / |   -.    |  / |  
            // | /  |          | /  |    
            ///|/   |          |/   |
            // 3----2          2----1 

            int V1, V2, V3, V4;

            V1 = TriA.m_Vertices[(EdgeA + 2) % 3];
            V2 = TriA.m_Vertices[EdgeA];
            V3 = TriB.m_Vertices[(EdgeB + 2) % 3];
            V4 = TriA.m_Vertices[(EdgeA + 1) % 3];

            // reconstruct the two triangles using the Quad V2, V3, V4, V1
            TriA.m_Vertices[0] = V2;
            TriA.m_Vertices[1] = V3;
            TriA.m_Vertices[2] = V1;

            TriB.m_Vertices[0] = V1;
            TriB.m_Vertices[1] = V3;
            TriB.m_Vertices[2] = V4;

            CalculateNormals();
            return true;
        }
        public virtual bool DivideEdge(int EdgeIndex, float Dist = 0.5f)
        {
            int TriNumA, EdgeA, VertNumA, VertNumB, n;
            MTriangle Tri = null;
            Vector3 VertA, VertB;
            byte oldEdgeFlags = 0;

            TriNumA = EdgeIndex / 3;
            EdgeA = EdgeIndex % 3;

            Tri = m_Tris[TriNumA];

            // we can't divide the edge if we have no triangle
            if (Tri == null)
                return false;

            // We now have one edge which we need to divide.
            VertNumA = Tri.m_Vertices[EdgeA];
            VertNumB = Tri.m_Vertices[(EdgeA + 1) % 3];

            VertA = GetVertexPosition(VertNumA);
            VertB = GetVertexPosition(VertNumB);

            // Now add in the new vertex
            Vector3 newVert;
            Vector3 edgeVec;
            int newVertIndex;

            edgeVec = VertB - VertA;
            newVert = Dist * edgeVec + VertA;
            newVertIndex = AddVertexArray(newVert);

            // now we loop over our entire mesh's triangles
            // and move triangles to suit the new location
            for (int triIndex = NumTris - 1; triIndex >= 0; --triIndex)
            {
                MTriangle tri = GetTri(triIndex);

                if (tri == null)
                    continue;

                int edgeNum;
                // see if we have our edge
                if ((edgeNum = tri.GetEdge(VertNumA, VertNumB)) != -1)
                {
                    MTriangle newTri;
                    byte newEdgeFlags = EDGE_VISIBLE;

                    // our new edge is invisible if any one of the other edges
                    // are invisible
                    if (!tri.edgeFlags[0].HasFlag(EDGE_VISIBLE) ||
                        !tri.edgeFlags[1].HasFlag(EDGE_VISIBLE) ||
                        !tri.edgeFlags[2].HasFlag(EDGE_VISIBLE))
                    {
                        newEdgeFlags = 0;
                    }

                    //spit the triangle at edgeNum
                    newTri = tri;
                    newTri.m_Vertices[edgeNum] = newVertIndex;

                    newTri.edgeFlags[(edgeNum + 2) % 3] = newEdgeFlags;

                    tri.m_Vertices[(edgeNum + 1) % 3] = newVertIndex;
                    tri.edgeFlags[(edgeNum + 1) % 3] = newEdgeFlags;

                    AddTriangleArray(newTri);
                }

            }

            CalculateNormals();

            // Mark just the new vertex with the FLAGFORCHAGE
            for (n = 0; n < NumVerts; n++)
                m_Verts[n].flags = m_Verts[n].flags.UnsetFlag(VERTEX_FLAGFORCHANGE);
            m_Verts[newVertIndex].flags = m_Verts[newVertIndex].flags.SetFlag(VERTEX_FLAGFORCHANGE);

            return true;
        }
        public virtual bool ExtrudeFaces(byte Flags, float Amount, Vector3 NormVec)
        {
            {
                //???MMeshEdgeData Edges;
                //???Edges.setVertexNum(NumVerts);
                //???for (int en = 0; en < NumTris; en++)
                //???    Edges.addFace(en, m_Tris[en]);
            }


            // What we have to do:
            //
            // Find all the vertices that are on the edge of the selection. These are the ones that 
            // Create new faces.
            //

            int n = 0, NumNewVerts;
            List<int> newVertsArray = new List<int>(); // This is an array of what the corresponding new vertices are.
            Vector3 OffsetVec;

            int EdgeCount = 0;
            OffsetVec = Amount * NormVec;
            Dictionary<int, List<int> > EdgeList = new Dictionary<int, List<int> >();
            var EdgeArray = new int[NumTris * 3];

            // Mark all the vertices of the extrusion
            {
                var VertArray = new int[NumVerts];

                {
                    int k = 0;
                    EdgeCount = 0;

                    // go through the selected faces, and add the edges in.
                    for (n = 0; n < NumTris; n++)
                    {
                        if (m_Tris[n].IsFlagged(Flags))
                        {
                            // If it is selected, try and add the edges
                            for (k = 0; k < 3; k++)
                            {
                                MTriangle Tri = m_Tris[n];
                                int V1, V2;
                                V1 = Tri.m_Vertices[k];
                                V2 = Tri.m_Vertices[(k + 1) % 3];

                                // See if the edge already exists in the list
                                if (EdgeList.Contains(V1, V2))
                                {
                                    EdgeList.Remove(V1, V2);
                                    EdgeCount--;
                                    continue;
                                }
                                if (EdgeList.Contains(V2, V1))
                                {
                                    EdgeList.Remove(V2, V1);
                                    EdgeCount--;
                                    continue;
                                }

                                // Add it to the list
                                EdgeList.Add(V2, V1);
                                EdgeCount++;
                            }
                        }
                    }

                    // Iterate through the edge list and mark all the vertices with -1.
                    for (n = 0; n < NumVerts; n++)
                        VertArray[n] = 1;
                    for (n = 0; n < NumVerts; n++)
                    {
                        if (EdgeList[n].Count > 0)
                        {
                            // iterate through the items
                            VertArray[n] = -1;
                            for (k = 0; k < EdgeList[n].Count; k++)
                                VertArray[EdgeList[n][k]] = -1;
                        }
                    }

                }

                // Now all the vertices with values of -1 are on the edge of the extrusion.
                // Makr them all accordingly
                NumNewVerts = 0;
                for (n = 0; n < NumVerts; n++)
                {
                    if (VertArray[n] < 0)
                    {
                        m_Verts[n].m_Pos = m_Verts[n].m_Pos + OffsetVec;
                    }

                    if (VertArray[n] == -1)
                    {
                        newVertsArray[n] = NumNewVerts + NumVerts;
                        NumNewVerts++;
                    }
                    else
                    {
                        newVertsArray[n] = -1;
                    }
                    m_Verts[n].UnsetFlag(VERTEX_FLAGFORCHANGE);
                    m_Verts[n].UnsetFlag(VERTEX_SELECTED);
                }
            }

            // Now we know which verts are changed and which stay the same.

            // Create the new vertex array.
            MTriangle[] NewTris = null, CurTri = null;
            var NewVerts = new Vector3[NumNewVerts];

            for (n = 0; n < NumVerts; n++)
            {
                if (newVertsArray[n] != -1)
                    NewVerts[newVertsArray[n] - NumVerts] = m_Verts[n].m_Pos + OffsetVec;
            }

            // Find out how many edges are in the extrusion, acreate the extra faces, and change the old verts

            {
                int k;

                NewTris = new MTriangle[EdgeCount * 2];    // We can at most have double the tris as verts
                                                           // We now have a list of edges that will make up the extruded edged faces.
                CurTri = NewTris;
                int triIdx = 0;
                for (n = 0; n < NumVerts; n++)
                {
                    int V1, V2;

                    if (EdgeList[n].Count == 0)
                        continue;

                    for (k = EdgeList[n].Count - 1; k >= 0; k--)
                    {
                        V1 = n;
                        V2 = EdgeList[n][k];

                        CurTri[triIdx].m_Vertices[0] = V1;
                        CurTri[triIdx].m_Vertices[1] = newVertsArray[V1];
                        CurTri[triIdx].m_Vertices[2] = newVertsArray[V2];
                        CurTri[triIdx].edgeFlags[2].UnsetFlag(EDGE_VISIBLE);
                        ++triIdx;

                        CurTri[triIdx].m_Vertices[0] = V1;
                        CurTri[triIdx].m_Vertices[1] = newVertsArray[V2];
                        CurTri[triIdx].m_Vertices[2] = V2;
                        CurTri[triIdx].edgeFlags[0].UnsetFlag(EDGE_VISIBLE);
                        ++triIdx;
                    }
                }

            }

            // Update the falgged tirangles to use the new vertices
            for (n = 0; n < NumVerts; n++)
            {
                if (!m_Tris[n].IsFlagged(Flags))
                    continue;

                for (int k = 0; k < 3; k++)
                {
                    if (m_Tris[n].m_Vertices[k] >= NumVerts)
                        continue;

                    if (newVertsArray[m_Tris[n].m_Vertices[k]] == -1)
                        continue;

                    m_Tris[n].m_Vertices[k] = newVertsArray[m_Tris[n].m_Vertices[k]];
                }
            }

            // Add the new arrays
            AddVertexArray(NewVerts);
            AddTriangleArray(NewTris);

            for (n = 0; n < newVertsArray.Count; n++)
            {
                if (newVertsArray[n] != -1)
                    SetVertexFlag(newVertsArray[n], (byte)(VERTEX_FLAGFORCHANGE | VERTEX_SELECTED));
            }

            // go through, and zero all the triangle vertex numbers, and add them to the texture mesh.
            //TODO{
            //TODO    //MEditableMeshPtr textureMesh;
            //TODO
            //TODO    var Tri = NewTris;
            //TODO    int triIdx = 0;
            //TODO    for (n = 0; n < EdgeCount; n++)
            //TODO    {
            //TODO        Tri[triIdx].m_Vertices[0] = 0;
            //TODO        Tri[triIdx].m_Vertices[1] = 0;
            //TODO        Tri[triIdx].m_Vertices[2] = 0;
            //TODO        ++triIdx;
            //TODO        Tri[triIdx].m_Vertices[0] = 0;
            //TODO        Tri[triIdx].m_Vertices[1] = 0;
            //TODO        Tri[triIdx].m_Vertices[2] = 0;
            //TODO        ++triIdx;
            //TODO    }
            //TODO
            //TODO    //textureMesh = AZTEC_CAST(MEditableMesh, getTextureMesh());
            //TODO    //TODO if (textureMesh != NULL)
            //TODO    //TODO {
            //TODO    //TODO     textureMesh->addTriangleArray(NewTris, EdgeCount * 2);
            //TODO    //TODO }
            //TODO}

            // Set the flag for Change flag on all the vertices
            // that sit on flagged triangles
            for (n = 0; n < NumVerts; n++)
            {
                if (!m_Tris[n].IsFlagged(Flags))
                {
                    continue;
                }

                m_Verts[m_Tris[n].m_Vertices[0]].SetFlag(VERTEX_FLAGFORCHANGE);
                m_Verts[m_Tris[n].m_Vertices[1]].SetFlag(VERTEX_FLAGFORCHANGE);
                m_Verts[m_Tris[n].m_Vertices[2]].SetFlag(VERTEX_FLAGFORCHANGE);
            }

            CalculateNormals();

            return true;
        }

        public virtual int SeparateFaces(byte Flags)
        {
            int NumFlaggedTris = 0, n, OldNumXYZ, NumSeamXYZ;
            MTriangle ppTri = null;

            NumFlaggedTris = 0;
            for (n = 0; n < NumTris; n++)
            {
                ppTri = m_Tris[n];
                if (ppTri == null)
                    continue;
                if (ppTri.flags.HasFlag(Flags))
                    NumFlaggedTris++;
            }

            if (NumFlaggedTris == 0)
                return 0;

            int[] VertsInfo = new int[NumVerts];
            for (int i = 0; i < VertsInfo.Length; ++i)
                VertsInfo[i] = 0;


            // Got through and flag the vertices if they are on a non-detached face
            // or on a detached face.  Those which are on both need to be duplicated.
            for (n = 0; n < NumTris; n++)
            {
                ppTri = m_Tris[n];
                if (ppTri == null)
                    continue;

                if (ppTri.flags.HasFlag(Flags))
                {
                    VertsInfo[ppTri.m_Vertices[0]] |= 0x01;
                    VertsInfo[ppTri.m_Vertices[1]] |= 0x01;
                    VertsInfo[ppTri.m_Vertices[2]] |= 0x01;
                }
                else
                {
                    VertsInfo[ppTri.m_Vertices[0]] |= 0x10;
                    VertsInfo[ppTri.m_Vertices[1]] |= 0x10;
                    VertsInfo[ppTri.m_Vertices[2]] |= 0x10;
                }
            }

            NumSeamXYZ = 0;
            for (n = 0; n < NumVerts; n++)
            {
                if (VertsInfo[n] == 0x11)
                    NumSeamXYZ++;
            }
            if (NumSeamXYZ == 0)
                return 0;

            OldNumXYZ = NumVerts;
            // Add the seam vertices to the current mesh
            {
                Vector3[] SeamVerts = new Vector3[NumSeamXYZ];

                NumSeamXYZ = 0;
                for (n = 0; n < NumVerts; n++)
                {
                    if (VertsInfo[n] == 0x11)
                    {
                        SeamVerts[NumSeamXYZ] = m_Verts[n].m_Pos;
                        NumSeamXYZ++;
                    }
                }
                AddVertexArray(SeamVerts);
            }


            // Go thorugh the VertsInfo array, and change it so that the array represents which of the enw vertices
            // should be used
            NumSeamXYZ = 0;
            for (n = 0; n < OldNumXYZ; n++)
            {
                if (VertsInfo[n] == 0x11)
                {
                    VertsInfo[n] = NumSeamXYZ;
                    NumSeamXYZ++;
                }
                else
                {
                    VertsInfo[n] = -1;
                }
            }

            // Go through the flagged triangles, and adjust the seam vertices so they are referencing
            // the newly made vertices
            for (n = 0; n < NumTris; n++)
            {
                ppTri = m_Tris[n];
                if (ppTri == null)
                    continue;

                if (ppTri.flags.HasFlag(Flags))
                {
                    for (int nvert = 0; nvert < 3; nvert++)
                    {
                        if (VertsInfo[ppTri.m_Vertices[nvert]] != -1)
                            ppTri.m_Vertices[nvert] = OldNumXYZ + VertsInfo[ppTri.m_Vertices[nvert]];
                    }
                }
            }
            return NumSeamXYZ;
        }
        public virtual bool CollapseVertices(byte Flags)
        {
            int NumCollapseVerts, n, NumNewVerts;
            Vector3 AveragePos = Vector3.Zero;

            // first, make sure there are some vertices to deal with.
            NumCollapseVerts = 0;
            for (n = 0; n < NumVerts; n++)
            {
                if (m_Verts[n].flags.HasFlag(Flags))
                {
                    NumCollapseVerts++;
                    AveragePos += m_Verts[n].m_Pos;
                }
            }

            if (NumCollapseVerts <= 1)
                return false;

            AveragePos *= 1.0f / NumCollapseVerts;
            NumNewVerts = NumVerts - NumCollapseVerts + 1;

            int[] CollapseTargets = new int[NumVerts];
            int FirstCollapseTarget = -1;
            NumCollapseVerts = 0;
            for (n = 0; n < NumVerts; n++)
            {
                if (m_Verts[n].flags.HasFlag(Flags))
                {
                    if (FirstCollapseTarget == -1)
                        FirstCollapseTarget = n;
                    else
                    {
                        NumCollapseVerts++;
                        //m_Verts.RemoveAt(n);
                    }
                    CollapseTargets[n] = FirstCollapseTarget;
                }
                else
                    CollapseTargets[n] = n - NumCollapseVerts;
            }

            // Create and fill up the new vertex array
            for (n = 0; n < NumVerts; n++)
                m_Verts[CollapseTargets[n]] = m_Verts[n];
            
            m_Verts[CollapseTargets[FirstCollapseTarget]] = m_Verts[FirstCollapseTarget];
            m_Verts[FirstCollapseTarget].m_Pos.X = AveragePos.X;
            m_Verts[FirstCollapseTarget].m_Pos.Y = AveragePos.Y;
            m_Verts[FirstCollapseTarget].m_Pos.Z = AveragePos.Z;

            // change the vertex numbers for all the triangles
            for (n = 0; n < NumTris; n++)
            {
                m_Tris[n].m_Vertices[0] = CollapseTargets[m_Tris[n].m_Vertices[0]];
                m_Tris[n].m_Vertices[1] = CollapseTargets[m_Tris[n].m_Vertices[1]];
                m_Tris[n].m_Vertices[2] = CollapseTargets[m_Tris[n].m_Vertices[2]];
            }

            m_Verts = m_Verts.GetRange(0, NumNewVerts);
            //CalculateNormals();

            return true;
        }

        public virtual int WeldVertices(byte Flags, float Threshold)
        {
            bool All = false;
            float Threshold2 = Threshold * Threshold;

            // If Flags is 0, then we are effecting all vertices.
            if (Flags == 0)
                All = true;
            else
                All = false;

            // Go through the mesh, and place all the vertices into various containers.
            int NumBoxesPerSide, n, i, j, k;

            NumBoxesPerSide = 1 + NumVerts / 500;
            var bounds = GetMeshExtents();
            Vector3 MeshMin = bounds.Min;
            Vector3 MeshMax = bounds.Max;

            MeshMin += new Vector3(-0.1f, -0.1f, -0.1f);
            MeshMax += new Vector3(0.1f, 0.1f, 0.1f);
            Vector3 BoxSize = MeshMax - MeshMin;
            BoxSize *= 1.0f / NumBoxesPerSide;

            // Create the array used to store the vertices
            var BoxVerts = new List<int>[NumBoxesPerSide, NumBoxesPerSide, NumBoxesPerSide];
            for (i = 0; i < NumBoxesPerSide; i++)
            {
                for (j = 0; j < NumBoxesPerSide; j++)
                    for (k = 0; k < NumBoxesPerSide; k++)
                        BoxVerts[i,j,k] = new List<int>();
            }

            // Go through the mesh, and place the vertices in the appropriate container.
            for (n = 0; n < NumVerts; ++n)
            {
                MInternalVertex V = null;
                int ip, jp, kp, inV, jn, kn;

                V = m_Verts[n];
                if (!(All || V.IsFlagged(Flags)))
                    continue;

                ip = (int)((V.m_Pos.X - MeshMin.X) / BoxSize.X);
                jp = (int)((V.m_Pos.Y - MeshMin.Y) / BoxSize.Y);
                kp = (int)((V.m_Pos.Z - MeshMin.Z) / BoxSize.Z);

                BoxVerts[ip, jp, kp].Add(n);
                for (i = -1; i <= 1; i++)
                {
                    for (j = -1; j <= 1; j++)
                    {
                        for (k = -1; k <= 1; k++)
                        {
                            inV = (int)((V.m_Pos.X - MeshMin.X + (Threshold * i)) / BoxSize.X);
                            jn = (int)((V.m_Pos.Y - MeshMin.Y + (Threshold * i)) / BoxSize.Y);
                            kn = (int)((V.m_Pos.Z - MeshMin.Z + (Threshold * i)) / BoxSize.Z);

                            if (inV != ip || jn != jp || kn != kp)
                                BoxVerts[inV,jn,kn].Add(n);
                        }
                    }
                }
            }


            // Now that we have boxified the vertex data, we can go through and weld the similar vertices.
            UnsetVertexFlags((byte)~VERTEX_VISIBLE);

            int[] TargetVerts = new int[NumVerts];
            for (i = 0; i < NumVerts; i++)
              TargetVerts[i] = -1;
  
            for (i = 0; i < NumBoxesPerSide; i++)
            {
                for (j = 0; j < NumBoxesPerSide; j++)
                {
                    for (k = 0; k < NumBoxesPerSide; k++)
                    {
                        int n1, n2;
                        int v1, v2;
                        var L = BoxVerts[i,j,k];
        
                        for (n1 = 0; n1 < L.Count; n1++)
                        {
                            v1 = L[n1];
                            for (n2 = n1 + 1; n2 < L.Count; n2++)
                            {
                                v2 = L[n2];
                                if (v1 == v2)
                                    continue;
                                float Len;
                                Vector3 DiffVec = m_Verts[v2].m_Pos - m_Verts[v1].m_Pos;
                                Len = DiffVec.LengthSquared();
                                if (Len <= Threshold2)
                                {
                                    if (TargetVerts[v2] == -1)
                                    {
                                        int Targ = v1;
                                        while (TargetVerts[Targ] != -1)
                                            Targ = TargetVerts[Targ];
                                        TargetVerts[v2] = Targ;
                                        m_Verts[v1].SetFlag(VERTEX_SELECTED);
                                        m_Verts[v1].SetFlag(VERTEX_SELECTED);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // go through and move all the triangles' vertices to the correct place.
            UnsetVertexFlags((byte)~VERTEX_VISIBLE);
            for (i = 0; i < NumVerts; i++)
            {
                if (TargetVerts[i] != -1)
                    m_Verts[i].SetFlag(VERTEX_FLAGFORCHANGE);
            }
  
            for (i = 0; i < NumVerts; i++)
            {
                for (k = 0; k < 3; k++)
                {
                    if ( TargetVerts[m_Tris[i].m_Vertices[k]] != -1)
                    {
                        m_Verts[m_Tris[i].m_Vertices[k]].SetFlag(VERTEX_FLAGFORCHANGE);
                        m_Tris[i].m_Vertices[k] = TargetVerts[m_Tris[i].m_Vertices[k]];
                    }
                }
            }
            for (i = 0; i < NumVerts; i++)
            {
                for (k=0; k<3; k++)
                    m_Verts[m_Tris[i].m_Vertices[k]].UnsetFlag(VERTEX_FLAGFORCHANGE);
            }

            DeleteVertexFlag(VERTEX_FLAGFORCHANGE);
            return 1;
        }

        #endregion
    }

    public static class DictExt
    {
        public static void Add(this Dictionary<int, List<int> > edgeList, int slot, int val)
        {
            if (!edgeList.ContainsKey(slot))
                edgeList[slot] = new List<int>();

            var list = edgeList[slot];
            list.Add(val);
        }

        public static void Remove(this Dictionary<int, List<int>> edgeList, int slot, int val)
        {
            if (!edgeList.ContainsKey(slot))
                return;
            var list = edgeList[slot];
            if (list != null)
                list.Remove(val);
        }

        public static bool Contains(this Dictionary<int, List<int>> edgeList, int slot, int val)
        {
            if (edgeList.ContainsKey(slot))
            {
                var list = edgeList[slot];
                return list.Contains(val);
            }
            return false;
        }
    }
}
