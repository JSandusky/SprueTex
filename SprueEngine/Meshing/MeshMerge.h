#pragma once

#include <SprueEngine/ClassDef.h>
#include <SprueEngine/Math/MathDef.h>
#include <SprueEngine/MathGeoLib/AllMath.h>

#include <vector>

namespace SprueEngine
{
    struct CSGPolygon;
    class MeshData;

    struct SPRUE CSGVertex
    {
        /// Vertex position
        Vec3 pos_;
        /// Vertex normal
        Vec3 normal_;
        /// -1k, -1k is our magic value for an unmapped UV coordinate.
        Vec2 uv_;
        /// Vertex came from Vertex N in the source mesh
        unsigned sourceIndex_ = -1;
        /// Vertex was created as a result of CSG
        bool isNew_ = false;
        CSGPolygon* polygon_ = 0x0;
        CSGVertex* firstColocal_ = 0x0;
        CSGVertex* nextColocal_ = 0x0;

        inline bool operator<(const CSGVertex& rhs) const
        {
            return std::tuple<Vec3, Vec3, Vec2>(pos_, normal_, uv_) <  std::tuple<Vec3, Vec3, Vec2>(rhs.pos_, rhs.normal_, rhs.uv_);
        }
    };

    /// Edges are used so that splits can be performed without T-junctions
    struct SPRUE CSGEdge
    {
        CSGVertex* start_ = 0x0;
        CSGVertex* end_ = 0x0;
        CSGEdge* opposite_ = 0x0;
        CSGPolygon* polygon_ = 0x0;

        void Split(float t);
    };

    struct SPRUE CSGModel
    {
        std::vector<CSGVertex> vertices_;
        std::vector<CSGEdge*> edges_;
        std::vector<int> indices_;
    };

    struct SPRUE CSGTriangle
    {
        math::Triangle triangle_;
        unsigned lastSplitter_ = -1;
        char backSideMarked_ = 0;
    };

    struct SPRUE CSGPolygon
    {
        CSGPolygon();
        CSGPolygon(const std::vector<CSGVertex>& list);

        /// Flips this polygon to face the opposite direction (also flips the vertex winding order)
        void Flip();
        Vec3 GetCentroid() const;
        Ray GetRay() const;

        /// List of vertices in the polygon.
        std::vector<CSGVertex> vertices_;
        /// Stored copy of the plane for this polygon.
        Plane plane_;
        /// Indicates which side of a cut operation this polygon was on, zero if it has never been cut.
        char backSideMarked_ = 0;
        unsigned lastSplitter_ = -1;

        bool Intersects(const CSGPolygon& rhs) const;
        bool ContainedBy(const BoundingBox& bounds) const;
    };

    /// Utility class responsible for the different fashions in which geometry can be merged.
    /// This includes simple buffer concatenation/flattening, CSG operations, hidden surface elimination, and clipping (one-sided CSG)
    class SPRUE MeshMerger
    {
    public:

        /// Able to identify if the receiver is the left-hand-side for reducing work.
        void CombineMeshes(MeshData* receiver, const MeshData* lhs, const MeshData* rhs, const Mat3x4& rhsTrans);
        
        /// Slice a model by a bunch of planes. Receiver is allowed to be the same as the 'toSlice' MeshData.
        void SliceModel(MeshData* receiver, const MeshData* toSlice, const std::vector<Plane>& slicePlanes);

        /// Construct a model for processing, must be done before starting CSG execution.
        void CSGPrepareModel(const MeshData* meshData, std::vector<CSGPolygon>& polygons, const Mat3x4& rhsTrans, bool includeUV) const;
        void CSGPrepareModel(const MeshData* meshData, std::vector<CSGTriangle>& traingles, const Mat3x4& rhsTrans) const;

        /// Convert from CSG Polygons into mesh data.
        void CSGExtractModel(MeshData* receivingMeshData, std::vector<CSGPolygon>& polygons, bool includeUV) const;
        void CSGExtractModel(MeshData* receivingMeshData, std::vector<CSGTriangle>& polygons) const;

        /// Builds a BVH for the triangles of each mesh and then repeatedly performs a CSG union.
        void CSGUnion(MeshData* receiver, const MeshData* lhs, const MeshData* rhs, const Mat3x4& rhsTrans, bool meshTogether);
        
        /// Builds a BVH for the triangles of each mesh and the repeatedly performs a CSG substraction.
        void CSGSubtract(MeshData* receiver, const MeshData* lhs, const MeshData* rhs, const Mat3x4& rhsTrans, bool meshTogether);

        /// Perferms a CSG intersection taking both negative sides.
        void CSGIntersect(MeshData* receiver, const MeshData* lhs, const MeshData* rhs, const Mat3x4& rhsTrans, bool meshTogether);

        void ClipTo(MeshData* receiver, const MeshData* lhs, const MeshData* rhs, const Mat3x4& rhsTrans, bool meshTogether);

        /// Performs a union on the given lists. Output is the passing polygons.
        void RawUnion(std::vector<CSGPolygon>& lhs, std::vector<CSGPolygon>& rhs);
        /// Performs a substraction of rhs from lhs. Output is the passing polygons.
        void RawSubstract(std::vector<CSGPolygon>& lhs, std::vector<CSGPolygon>& rhs);
        /// Intersects the left and right polygons. Output is the passing polygons.
        void RawIntersect(std::vector<CSGPolygon>& lhs, std::vector<CSGPolygon>& rhs);

        /// Filters a list of polygons to only those intersecting the bounding box. Only use on polygons that are triangular in order to reduce the number of triangle-to-triangle comparisons
        std::vector<CSGPolygon> Select(std::vector<CSGPolygon>& list, BoundingBox bounds, bool erase = false /* selected polygons will be removed from the list */);

    private:
        void SplitTriangles(std::vector<CSGTriangle>& lhs, std::vector<CSGTriangle>& rhs);
        void SplitPolygons(std::vector<CSGPolygon>& lhs, std::vector<CSGPolygon>& rhs, const BoundingBox& bounds) const;

        bool SplitPolygon(const Plane& plane, CSGPolygon& polygon, std::vector<CSGPolygon>& polygons, unsigned polyID) const;

        void EliminatePolygons(std::vector<CSGPolygon>& polygons, bool inside, bool flipPassing = false) const;
        void EliminateTriangles(std::vector<CSGTriangle>& triangles, bool inside, bool flipPassing = false) const;
    };

}