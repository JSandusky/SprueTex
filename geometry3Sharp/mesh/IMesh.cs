﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace g3
{
    public interface IPointMetaData
    {
        bool HasComponent(int vertexIdx);
        bool Interpolate(int from, int start, int writeInto = -1);
        Vector3f GetVector3f(int vertexIdx);
        Vector2f GetVector2f(int vertexidx);
        int GetInt(int vertexIdx);
    }

    public abstract class BaseMetaData : IPointMetaData
    {
        Dictionary<int, object> dataStore_ = new Dictionary<int, object>();
        public int GetInt(int vertexIdx)
        {
            object val = null;
            if (dataStore_.TryGetValue(vertexIdx, out val))
            {
                if (val != null)
                    return (int)val;
            }
            return -1;
        }

        public Vector2f GetVector2f(int vertexidx)
        {
            object val = null;
            if (dataStore_.TryGetValue(vertexidx, out val))
            {
                if (val != null)
                    return (Vector2f)val;
            }
            return Vector2f.Zero;
        }

        public Vector3f GetVector3f(int vertexIdx)
        {
            object val = null;
            if (dataStore_.TryGetValue(vertexIdx, out val))
            {
                if (val != null)
                    return (Vector3f)val;
            }
            return Vector3f.Zero;
        }

        public bool HasComponent(int vertexIdx)
        {
            object val = null;
            if (dataStore_.TryGetValue(vertexIdx, out val))
                return val != null;
            return false;
        }

        public abstract bool Interpolate(int from, int start, int writeInto = -1);
    }

    public interface IPointSet
    {
        int VertexCount { get; }
		int MaxVertexID { get; }

        bool HasVertexNormals { get; }
        bool HasVertexColors { get; }

        Vector3d GetVertex(int i);
        Vector3f GetVertexNormal(int i);
        Vector3f GetVertexColor(int i);

        bool IsVertex(int vID);

        // iterators allow us to work with gaps in index space
        System.Collections.Generic.IEnumerable<int> VertexIndices();
    }



    public interface IMesh : IPointSet
    {
        int TriangleCount { get; }
		int MaxTriangleID { get; }

        bool HasVertexUVs { get; }
        Vector2f GetVertexUV(int i);

        NewVertexInfo GetVertexAll(int i);

        bool HasTriangleGroups { get; }

        Index3i GetTriangle(int i);
        int GetTriangleGroup(int i);

        bool IsTriangle(int tID);

        // iterators allow us to work with gaps in index space
        System.Collections.Generic.IEnumerable<int> TriangleIndices();
    }


    public interface IDeformableMesh : IMesh
    {
        void SetVertex(int vID, Vector3d vNewPos);
        void SetVertexNormal(int vid, Vector3f vNewNormal);
    }



    /*
     * Abstracts construction of meshes, so that we can construct different types, etc
     */
    public struct NewVertexInfo
    {
        public Vector3d v;
        public Vector3f n, c;
        public Vector2f uv;
        public bool bHaveN, bHaveUV, bHaveC;

		public NewVertexInfo(Vector3d v) {
			this.v = v; n = c = Vector3f.Zero; uv = Vector2f.Zero;
			bHaveN = bHaveC = bHaveUV = false;
		}
		public NewVertexInfo(Vector3d v, Vector3f n) {
			this.v = v; this.n = n; c = Vector3f.Zero; uv = Vector2f.Zero;
			bHaveN = true; bHaveC = bHaveUV = false;
		}
		public NewVertexInfo(Vector3d v, Vector3f n, Vector3f c) {
			this.v = v; this.n = n; this.c = c; uv = Vector2f.Zero;
			bHaveN = bHaveC = true; bHaveUV = false;
		}
		public NewVertexInfo(Vector3d v, Vector3f n, Vector3f c, Vector2f uv) {
			this.v = v; this.n = n; this.c = c; this.uv = uv;
			bHaveN = bHaveC = bHaveUV = true;
		}
    }


    public interface IMeshBuilder
    {
        // return ID of new mesh
        int AppendNewMesh(bool bHaveVtxNormals, bool bHaveVtxColors, bool bHaveVtxUVs, bool bHaveFaceGroups);
        void SetActiveMesh(int id);

        int AppendVertex(double x, double y, double z);
        int AppendVertex(NewVertexInfo info);

        int AppendTriangle(int i, int j, int k);
        int AppendTriangle(int i, int j, int k, int g);


        // material handling

        // return client-side unique ID of material
        int BuildMaterial(GenericMaterial m);

        // do material assignment to mesh, where meshID comes from IMeshBuilder
        void AssignMaterial(int materialID, int meshID);

        // optional
        bool SupportsMetaData { get; }
        void AppendMetaData(string identifier, object data);
    }




}
