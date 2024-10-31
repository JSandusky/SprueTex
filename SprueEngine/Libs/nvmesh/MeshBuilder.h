// This code is in the public domain -- castanyo@yahoo.es

#pragma once
#ifndef NV_MESH_MESHBUILDER_H
#define NV_MESH_MESHBUILDER_H

//#include "nvmesh.h"
//#include "nvcore/Array.h"
//#include "nvmath/Vector.h"

#include <SprueEngine/MathGeoLib/AllMath.h>

#include <vector>

#define NIL uint32_t(~0)

namespace nv
{
    class TriMesh;
    class QuadTriMesh;
    namespace HalfEdge { class Mesh; }
    namespace Reducer { class Mesh; }


    /// Mesh builder is a helper class for importers.
    /// Ideally it should handle any vertex data, but for now it only accepts positions, 
    /// normals and texcoords.
    class MeshBuilder
    {
        MeshBuilder(const MeshBuilder&);
        //NV_FORBID_HEAPALLOC();
    public:
        MeshBuilder();
        ~MeshBuilder();

        // Builder methods.
        uint32_t addPosition(const SprueEngine::Vec3 & v);
        uint32_t addNormal(const SprueEngine::Vec3 & v);
        uint32_t addTexCoord(const SprueEngine::Vec2 & v, uint32_t set = 0);
        uint32_t addColor(const SprueEngine::Vec4& v, uint32_t set = 0);

        void beginGroup(uint32_t id);
        void endGroup();

        uint32_t addMaterial(const char * name);
        void beginMaterial(uint32_t id);
        void endMaterial();

        void beginPolygon(uint32_t id = 0);
        uint32_t addVertex(uint32_t p, uint32_t n = NIL, uint32_t t0 = NIL, uint32_t t1 = NIL, uint32_t c0 = NIL, uint32_t c1 = NIL, uint32_t c2 = NIL);
        uint32_t addVertex(const SprueEngine::Vec3 & p);
        //uint addVertex(const SprueEngine::Vec3 & p, const SprueEngine::Vec3 & n, const SprueEngine::Vec2 & t0 = SprueEngine::Vec2(0), const SprueEngine::Vec2 & t1 = SprueEngine::Vec2(0), const SprueEngine::Vec4 & c0 = SprueEngine::Vec4(0), const SprueEngine::Vec4 & c1 = SprueEngine::Vec4(0));
        bool endPolygon();

        uint32_t weldPositions();
        uint32_t weldNormals();
        uint32_t weldTexCoords(uint32_t set = 0);
        uint32_t weldColors(uint32_t set = 0);
        void weldVertices();

        void optimize(); // eliminate duplicate components and duplicate vertices.
        void removeUnusedMaterials(std::vector<uint32_t> & newMaterialId);
        void sortFacesByGroup();
        void sortFacesByMaterial();

        void done();
        void reset();

        // Hints.
        void hintTriangleCount(uint32_t count);
        void hintVertexCount(uint32_t count);
        void hintPositionCount(uint32_t count);
        void hintNormalCount(uint32_t count);
        void hintTexCoordCount(uint32_t count, uint32_t set = 0);
        void hintColorCount(uint32_t count, uint32_t set = 0);

        // Helpers.
        void addTriangle(uint32_t v0, uint32_t v1, uint32_t v2);
        void addQuad(uint32_t v0, uint32_t v1, uint32_t v2, uint32_t v3);

        // Get result.
        TriMesh * buildTriMesh() const;
        QuadTriMesh * buildQuadTriMesh() const;

        enum Error {
            Error_None,
            Error_NonManifoldEdge,
            Error_NonManifoldVertex,
        };

        HalfEdge::Mesh * buildHalfEdgeMesh(bool weldPositions, Error * error = NULL, std::vector<uint32_t> * badFaces = NULL) const;

        Reducer::Mesh * buildReducerMesh(bool weldPositions, Error * error = NULL, std::vector<uint32_t> * badFaces = NULL) const;

        bool buildPositions(std::vector<SprueEngine::Vec3> & positionArray);
        bool buildNormals(std::vector<SprueEngine::Vec3> & normalArray);
        bool buildTexCoords(std::vector<SprueEngine::Vec2> & texCoordArray, uint32_t set = 0);
        bool buildColors(std::vector<SprueEngine::Vec4> & colorArray, uint32_t set = 0);
		void buildVertexToPositionMap(std::vector<int> & map);


        // Expose attribute indices of the unified vertex array.
        uint32_t vertexCount() const;
        
        uint32_t positionCount() const;
        uint32_t normalCount() const;
        uint32_t texCoordCount(uint32_t set = 0) const;
        uint32_t colorCount(uint32_t set = 0) const;

        uint32_t materialCount() const;
        const char * material(uint32_t i) const;

        uint32_t positionIndex(uint32_t vertex) const;
        uint32_t normalIndex(uint32_t vertex) const;
        uint32_t texCoordIndex(uint32_t vertex, uint32_t set = 0) const;
        uint32_t colorIndex(uint32_t vertex, uint32_t set = 0) const;

    private:

        struct PrivateData;
        PrivateData * d;

    };

} // nv namespace

#endif // NV_MESH_MESHBUILDER_H
