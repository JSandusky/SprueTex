#include "MeshMerge.h"

#include <SprueEngine/Geometry/MeshData.h>
#include <SprueEngine/MathGeoLib/Geometry/LineSegment.h>
#include <SprueEngine/MathGeoLib/Geometry/Polygon.h>

#include <SprueEngine/Libs/nvmesh/halfedge/Edge.h>
#include <SprueEngine/Libs/nvmesh/halfedge/Face.h>
#include <SprueEngine/Libs/nvmesh/halfedge/Mesh.h>
#include <SprueEngine/Libs/nvmesh/halfedge/Vertex.h>
#include <SprueEngine/Libs/nvmesh/MeshBuilder.h>
#include <SprueEngine/Libs/nvmesh/param/Util.h>
#include <SprueEngine/Meshing/CSG.h>

#include <SprueEngine/MathGeoLib/Geometry/Polygon.h>

#include <algorithm>
#include <functional>
#include <unordered_map>


inline size_t junk_fnv1a_hash_bytes(const unsigned char * first, size_t count) {
#if defined(_WIN64)
    static_assert(sizeof(size_t) == 8, "This code is for 64-bit size_t.");
    const size_t fnv_offset_basis = 14695981039346656037ULL;
    const size_t fnv_prime = 1099511628211ULL;
#else /* defined(_WIN64) */
    static_assert(sizeof(size_t) == 4, "This code is for 32-bit size_t.");
    const size_t fnv_offset_basis = 2166136261U;
    const size_t fnv_prime = 16777619U;
#endif /* defined(_WIN64) */

    size_t result = fnv_offset_basis;
    for (size_t next = 0; next < count; ++next)
    {
        // fold in another byte
        result ^= (size_t)first[next];
        result *= fnv_prime;
    }
    return (result);
}

template<> struct std::hash<SprueEngine::Vec3>
{
    size_t operator()(const SprueEngine::Vec3& x) const { return junk_fnv1a_hash_bytes((unsigned char*)&x, sizeof(SprueEngine::Vec3)); }
};

template<>
struct std::hash<std::pair<unsigned, unsigned> >
{
    size_t operator()(const std::pair<unsigned, unsigned>& d) const {
        return junk_fnv1a_hash_bytes((unsigned char*)&d, sizeof(std::pair<unsigned, unsigned>));
    }
};

namespace SprueEngine
{

#define CSG_EPSILON 0.00001f
#define MARK_BACK_SIDE(CURRENT, COUNTS) (-128 + COUNTS >= CURRENT ? CURRENT : CURRENT - COUNTS)
#define MARK_FRONT_SIDE(CURRENT, COUNTS) (128 - COUNTS >= CURRENT ? CURRENT : CURRENT + COUNTS)

    void EliminateByWinding(std::vector<CSGPolygon>& polygons, const MeshData* rhs, const Mat3x4& rhsTrans, bool inside, bool flipPassing)
    {
        std::vector<char> polyMarks(polygons.size(), 0);
        for (unsigned i = 0; i < polygons.size(); ++i)
        {
            Ray castRay = polygons[i].GetRay();
            unsigned winding = 0;
            rhs->SweepRayForWinding(castRay, winding, rhsTrans);

            // if > 0 then it's inside
            if (winding > 0)
                polyMarks[i] = -1;
            else
                polyMarks[i] = 1;
        }

        std::vector<CSGPolygon> outPolys;
        outPolys.reserve(polygons.size());

        const char passingMark = inside ? -1 : 1;
        for (unsigned i = 0; i < polygons.size(); ++i)
            if (polyMarks[i] == passingMark)
            {
                if (flipPassing)
                    polygons[i].Flip();
                outPolys.push_back(polygons[i]);
            }

        polygons = outPolys;
    }

    void EliminateByVertex(std::vector<CSGPolygon>& polygons, const std::vector<CSGPolygon>& compareSet, bool inside, bool flipPassing)
    {
        // Fill the initial set
        std::vector<char> polyMarks(polygons.size(), 0);
        for (unsigned i = 0; i < polygons.size(); ++i)
            polyMarks[i] = polygons[i].backSideMarked_;

        for (unsigned i = 0; i < polygons.size(); ++i)
        {
            unsigned winding = 0;
            //for (unsigned j = 0; j < compareSet.size(); ++j)
            //{
            //    for (auto& vert : polygons[i].vertices_)
            //    {
            //        if (compareSet[j].plane_.SignedDistance(vert.pos_) > CSG_EPSILON)
            //            ++winding;
            //        else
            //            --winding;
            //    }
            //}
            //if (winding % 2)
            //    polyMarks[i] = -1;
            //else
            //    polyMarks[i] = 1;
        }

        // Take only the polygons whose mark has the desired sign
        std::vector<CSGPolygon> outPolys;
        outPolys.reserve(polygons.size());

        const unsigned char passingMark = inside ? -1 : 1;
        for (unsigned i = 0; i < polygons.size(); ++i)
            if (polyMarks[i] == passingMark)
            {
                if (flipPassing)
                    polygons[i].Flip();
                outPolys.push_back(polygons[i]);
            }

        polygons = outPolys;
    }

    nv::HalfEdge::Mesh* CSGPolygonsToHalfEdgeMesh(const std::vector<CSGPolygon>& polygons)
    {
        nv::HalfEdge::Mesh* mesh = new nv::HalfEdge::Mesh();

        for (const auto& polygon : polygons)
        {
            if (polygon.vertices_.size() < 3)
                continue;
            uint32_t startIndex = mesh->vertexCount();
            for (const auto& vertex : polygon.vertices_)
                mesh->addVertex(vertex.pos_);
        }

        mesh->linkColocals();

        std::vector<uint32_t> indices;
        uint32_t current = 0;
        indices.reserve(32);
        for (const auto& polygon : polygons)
        {
            if (polygon.vertices_.size() < 3)
                continue;
            for (uint32_t i = 0; i < polygon.vertices_.size(); ++i)
                indices.push_back(current + i);
            if (mesh->addFace(indices))
                current += polygon.vertices_.size();

            indices.clear();
        }

        mesh->linkBoundary();

        return mesh;
    }

    inline static CSGVertex Interpolate(const CSGVertex& a, const CSGVertex& b, float t)
    {
        CSGVertex ret;
        ret.pos_ = SprueLerp(a.pos_, b.pos_, t);
        ret.normal_ = SprueLerp(a.normal_, b.normal_, t);
        ret.uv_ = SprueLerp(a.uv_, b.uv_, t);
        ret.isNew_ = true;
        return ret;
    }

    void CSGEdge::Split(float t)
    {
        CSGVertex newVert = Interpolate(*start_, *end_, t);
        if (opposite_)
            opposite_->Split(1.0f - t);
    }

    CSGPolygon::CSGPolygon()
    {

    }

    CSGPolygon::CSGPolygon(const std::vector<CSGVertex>& list) : 
        vertices_(list),
        plane_(vertices_[0].pos_, vertices_[1].pos_, vertices_[2].pos_)
    {

    }

    void CSGPolygon::Flip()
    {
        std::reverse(vertices_.begin(), vertices_.end());
        for (size_t i = 0; i < vertices_.size(); i++)
            vertices_[i].normal_ = -vertices_[i].normal_;
        plane_.ReverseNormal();
    }

    Vec3 CSGPolygon::GetCentroid() const
    {
        Vec3 sum = vertices_.front().pos_;
        for (unsigned i = 1; i < vertices_.size(); ++i)
            sum += vertices_[i].pos_;
        return sum / vertices_.size();
    }

    Ray CSGPolygon::GetRay() const
    {
        return Ray(GetCentroid(), plane_.normal);
    }

    bool CSGPolygon::Intersects(const CSGPolygon& other) const
    {
        Polygon left, right;
        if (vertices_.empty())
            return false;
        if (other.vertices_.empty())
            return false;
        for (auto vert : vertices_)
            left.p.push_back(vert.pos_.ToPos4());
        for (auto vert : other.vertices_)
            right.p.push_back(vert.pos_.ToPos4());

        return left.Intersects(right);
    }

    bool CSGPolygon::ContainedBy(const BoundingBox& bounds) const
    {
        Polygon poly;
        for (unsigned i = 0; i < vertices_.size(); ++i)
            poly.p.push_back(vertices_[i].pos_.ToPos4());

        return bounds.Intersects(poly);
    }

    // Used instead of nv::HalfEdge::Mesh to eliminate excessive duplication of polygon data
    struct CSGLocality
    {
        struct CSGEdge
        {
            /// The pair edge is colinear and goes the other direction
            CSGEdge* pair = 0x0;
            /// The next edge in the polygon loop
            CSGEdge* next = 0x0;
            /// The polygon this edge wraps around in face winding order (order determined by input polygons), edge may have multiples in the case of non-manifold T-shapes
            CSGPolygon* polygon = 0x0;
            uint32_t polygonIndex = -1;
        };

        /// Contains the first appearance index of a vertex at a given position
        std::map<SprueEngine::Vec3, std::vector<unsigned> > colocalVertices;
        /// Maps a pair of colocal indices (head -> tail) to an actual edge
        std::unordered_map<std::pair<unsigned, unsigned>, CSGEdge*> edges;
        /// List of the 'first edge' for each polgyon
        std::vector<CSGEdge*> firstEdges;

        /// Construct and fill data.
        CSGLocality(std::vector<CSGPolygon>& polygons)
        {
            firstEdges.resize(polygons.size(), 0x0);
            for (unsigned i = 0; i < polygons.size(); ++i)
                AddPolygon(polygons[i], i);
        }

        CSGLocality(std::vector<CSGTriangle>& triangles)
        {
            for (unsigned i = 0; i < triangles.size(); ++i)
            {
                AddVertex(triangles[i].triangle_.a, 0, i);
                AddVertex(triangles[i].triangle_.b, 0, i);
                AddVertex(triangles[i].triangle_.c, 0, i);
            }
        }

        ~CSGLocality()
        {
            // Only data allocated is edges
            for (auto record : edges)
                delete record.second;
        }

        /// Adds a polygons vertices, followed by its edges
        void AddPolygon(CSGPolygon& poly, unsigned polyIndex)
        {
            if (poly.vertices_.size() < 3)
                return;

            std::vector<unsigned> vertexIds(poly.vertices_.size());
            for (unsigned i = 0; i < poly.vertices_.size(); ++i)
                AddVertex(poly.vertices_[i], i, polyIndex);
            
            //CSGEdge* last = 0x0;
            //for (unsigned i = 0; i < vertexIds.size() - 1; ++i)
            //{
            //    CSGEdge* newEdge = AddEdge(poly, polyIndex, vertexIds[i], vertexIds[i + 1]);
            //    if (last == 0x0)
            //        firstEdges[polyIndex] = newEdge;
            //    else
            //        last->next = newEdge;
            //    last = newEdge;
            //}
            //CSGEdge* finalEdge = AddEdge(poly, polyIndex, vertexIds.back(), vertexIds.front());
            //last->next = finalEdge;
            //finalEdge->next = firstEdges[polyIndex];
        }

        /// Add a vertex if needed then return the colocal index
        void AddVertex(CSGVertex& vert, unsigned index, unsigned polyIndex)
        {
            auto found = colocalVertices.find(vert.pos_);
            if (found != colocalVertices.end())
                found->second.push_back(polyIndex);
            else
            {
                std::vector<unsigned> insert;
                insert.push_back(polyIndex);
                colocalVertices[vert.pos_] = insert;
            }
        }

        void AddVertex(const Vec3& vert, unsigned index, unsigned polyIndex)
        {
            auto found = colocalVertices.find(vert);
            if (found != colocalVertices.end())
                found->second.push_back(polyIndex);
            else
            {
                std::vector<unsigned> insert;
                insert.push_back(polyIndex);
                colocalVertices[vert] = insert;
            }
        }

        /// Add an edge, connect pairs as needed
        CSGEdge* AddEdge(CSGPolygon& poly, unsigned polyIndex, unsigned startVert, unsigned endVert)
        {
            auto existing = edges.find(std::make_pair(startVert, endVert));
            if (existing != edges.end())
                return existing->second;

            CSGEdge* newEdge = new CSGEdge();
            newEdge->polygon = &poly;
            newEdge->polygonIndex = polyIndex;

            auto foundPair = edges.find(std::make_pair(endVert, startVert));
            if (foundPair != edges.end())
            {
                newEdge->pair = foundPair->second;
                foundPair->second->pair = newEdge;
            }

            edges[std::make_pair(startVert, endVert)] = newEdge;
            return newEdge;
        }

        /// Loops over the edges of a polygon in order to get to the polygons of their pairs
        struct NeighborIterator {
            /// Construct and assign the current edge
            NeighborIterator(CSGEdge* edge) : current(edge) { }

            /// If no edge was found we'll be done before ever starting, making this an island
            bool IsDone() {
                return current == end;
            }

            /// Move to the next neighbor, set the end if not already set
            void Advance() {
                if (end == 0x0)
                    end = current;
                current = current->next;
            }

            /// Returns the current polygon if it exists
            CSGPolygon* Current() {
                if (current && current->pair)
                    return current->pair->polygon;
                return 0x0;
            }

            uint32_t CurrentIndex() {
                if (current && current->pair)
                    return current->pair->polygonIndex;
                return -1;
            }

        private:
            CSGEdge* end = 0x0;
            CSGEdge* current = 0x0;
        };

        /// Get a neighbors iterator for the polygon at the given index
        NeighborIterator Neighbors(unsigned polyIndex) { return NeighborIterator(firstEdges[polyIndex]); }
    };

    template<typename T>
    void MergeVectors(unsigned leftVC, unsigned rightVC, std::vector<T>& left, const std::vector<T>& right)
    {
        if (left.empty() && !right.empty())
        {
            left.resize(leftVC);
            left.insert(left.end(), right.begin(), right.end());
        }
        else if (!left.empty() && right.empty())
        {
            auto r = right;
            r.resize(rightVC);
            left.insert(left.end(), r.begin(), r.end());
        }
        else if (!left.empty() && !right.empty())
        {
            left.insert(left.end(), right.begin(), right.end());
        }
        else
            return;
    }

    void MeshMerger::CombineMeshes(MeshData* receiver, const MeshData* lhs, const MeshData* rhs, const Mat3x4& rhsTrans)
    {
        unsigned indexStart = receiver->positionBuffer_.size();
        unsigned leftC = lhs->positionBuffer_.size();
        unsigned rightC = rhs->positionBuffer_.size();
        auto MergeInto = [&indexStart, &leftC, &rightC](MeshData* into, const MeshData* src, const Mat3x4& rhsTrans) {
            
            std::vector<Vec3> rhsPos(src->positionBuffer_);
            std::vector<Vec3> rhsNor(src->normalBuffer_);

            for (int i = 0; i < rhsPos.size(); ++i)
                rhsPos[i] = rhsPos[i] * rhsTrans;

            auto rotMat = rhsTrans.RotatePart();
            for (int i = 0; i < rhsNor.size(); ++i)
                rhsNor[i] = rhsNor[i] * rotMat;

            // Helper macro for merging vertex data arrays nicely
#define APPEND_BUFFER(BUFFER, DEFELEM) if (indexStart && !into-> BUFFER .empty() && src-> BUFFER .empty()) { into->BUFFER .resize(into->BUFFER.size() + src->positionBuffer_.size(), DEFELEM); } else into->BUFFER.insert(into-> BUFFER .end(), src-> BUFFER .begin(), src-> BUFFER .end())

            MergeVectors<Vec3>(leftC, rightC, into->positionBuffer_, rhsPos);
            MergeVectors<Vec3>(leftC, rightC, into->normalBuffer_, rhsNor);
            MergeVectors<Vec2>(leftC, rightC, into->uvBuffer_, src->uvBuffer_);
            MergeVectors<RGBA>(leftC, rightC, into->colorBuffer_, src->colorBuffer_);
            MergeVectors<Vec4>(leftC, rightC, into->tangentBuffer_, src->tangentBuffer_);

            MergeVectors<Vec4>(leftC, rightC, into->boneWeights_, src->boneWeights_);
            MergeVectors<IntVec4>(leftC, rightC, into->boneIndices_, src->boneIndices_);

            //APPEND_BUFFER(positionBuffer_, Vec3(0,0,0));
            //APPEND_BUFFER(normalBuffer_, Vec3(0,0,0));
            //APPEND_BUFFER(uvBuffer_, Vec2(0,0));
            //APPEND_BUFFER(colorBuffer_, RGBA(0.0f, 0.0f, 0.0f));
            //APPEND_BUFFER(tangentBuffer_, Vec4(0, 0, 0, 0));
            //
            //// Bones probably require some extra checks
            //APPEND_BUFFER(boneIndices_, IntVec4());
            //APPEND_BUFFER(boneWeights_, Vec4(0, 0, 0, 0));

            std::vector<unsigned> indices = src->indexBuffer_;
            for (unsigned i = 0; i < indices.size(); ++i)
                indices[i] = indices[i] + indexStart;
            into->indexBuffer_.insert(into->indexBuffer_.end(), indices.begin(), indices.end());
        };

        //if (receiver != lhs)
        //    MergeInto(receiver, lhs);
        MergeInto(receiver, rhs, rhsTrans);
    }

    void SplitTriangle(math::Triangle& left, math::Triangle& right)
    {
        math::LineSegment segment;
        if (right.Intersects(left, &segment))
        {
            math::Triangle lA, lB;
            math::Triangle rA, rB;
            
            math::Plane rhsPlane = right.PlaneCCW();
            int leftCt = rhsPlane.Clip(left, lA, lB);
            rhsPlane = left.PlaneCCW();
            int rightCt = rhsPlane.Clip(right, rA, rB);
        }
    }

    void MeshMerger::SliceModel(MeshData* receiver, const MeshData* toSlice, const std::vector<Plane>& slicePlanes)
    {
        std::vector<CSGPolygon> data;
        CSGPrepareModel(toSlice, data, Mat3x4::identity, true);
        for (unsigned i = 0; i < data.size(); ++i)
        {
            for (unsigned p = 0; p < slicePlanes.size(); ++p)
            {
                if (SplitPolygon(slicePlanes[p], data[i], data, -1))
                {
                    data.erase(data.begin() + i);
                    --i;
                }
            }
        }
    }

    void MeshMerger::CSGPrepareModel(const MeshData* mesh, std::vector<CSGPolygon>& polygons, const Mat3x4& rhsTrans, bool includeUV) const
    {
        if (mesh == 0x0)
            return;

        auto rotMat = rhsTrans.RotatePart();
        for (size_t i = 0; i < mesh->indexBuffer_.size(); i += 3)
        {
            std::vector<CSGVertex> triangle(3);
            for (int j = 0; j < 3; j++)
            {
                const unsigned idx = mesh->indexBuffer_[i + j];
                triangle[j].sourceIndex_ = idx;
                triangle[j].pos_ = rhsTrans.MulPos(mesh->positionBuffer_[idx]);
                if (!mesh->normalBuffer_.empty())
                    triangle[j].normal_ = rotMat.MulDir(mesh->normalBuffer_[idx]).Normalized();
                if (includeUV && mesh->uvBuffer_.size())
                    triangle[j].uv_ = mesh->uvBuffer_[idx];
            }
            polygons.push_back(CSGPolygon(triangle));
        }
    }

    void MeshMerger::CSGPrepareModel(const MeshData* mesh, std::vector<CSGTriangle>& triangles, const Mat3x4& rhsTrans) const
    {
        if (mesh == 0x0)
            return;

        for (size_t i = 0; i < mesh->indexBuffer_.size(); i += 3)
        {
            CSGTriangle triangle;
            for (int j = 0; j < 3; j++)
            {
                const unsigned idx = mesh->indexBuffer_[i + j];
                //triangle[j].sourceIndex_ = idx;
                triangle.triangle_.VertexArrayPtr()[j] = mesh->positionBuffer_[idx].ToPos4() * rhsTrans;
                //triangle[j].pos_ = mesh->positionBuffer_[idx];
                //triangle[j].normal_ = mesh->normalBuffer_[idx];
                //if (includeUV && mesh->uvBuffer_.size())
                //    triangle[j].uv_ = mesh->uvBuffer_[idx];
            }
            triangles.push_back(triangle);
        }
    }

    typedef std::map<CSGVertex, unsigned> CSGColocalMap;

    unsigned CSGAddVertex(CSGVertex& vert, CSGColocalMap& colocalTable, std::vector<CSGVertex>& allVerts, SprueEngine::MeshData* meshData)
    {
        auto found = colocalTable.find(vert);
        if (found == colocalTable.end())
        {
            unsigned idx = allVerts.size();
            allVerts.push_back(vert);
            colocalTable[vert] = idx;

            meshData->positionBuffer_.push_back(vert.pos_);
            meshData->normalBuffer_.push_back(vert.normal_);
            meshData->uvBuffer_.push_back(vert.uv_);

            return idx;
        }
        return found->second;
    }

    //CSGVertex CSGGetFaceVertex(CSGMeshType::face_t* face, unsigned vertIdx, CSGMeshType::vertex_t* vert, CSGAttributes* attr)
    //{
    //    CSGVertex ret;
    //    ret.pos = Vec3(vert->v[0], vert->v[1], vert->v[2]);
    //    Vec3 nor = attr->NormalsAttr.getAttribute(face, vertIdx);
    //    if (nor.Dot(Vec3(face->plane.N[0], face->plane.N[1], face->plane.N[2]).Normalized()) < 0)
    //        nor *= -1;
    //    ret.nor = nor.Normalized();
    //    ret.uv = attr->UVAttr.getAttribute(face, vertIdx);
    //    return ret;
    //}

    void MeshMerger::CSGExtractModel(MeshData* receivingMesh, std::vector<CSGPolygon>& polygons, bool includeUV) const
    {
#if 1
        receivingMesh->Clear();
        CSGModel model;
        CSGColocalMap colocalTable;
        std::vector<CSGVertex> allVerts;
        int p = 0;
        for (size_t i = 0; i < polygons.size(); i++)
        {
            CSGPolygon& poly = polygons[i];
            for (size_t j = 2; j < poly.vertices_.size(); j++)
            {
                unsigned iA = CSGAddVertex(poly.vertices_[0], colocalTable, allVerts, receivingMesh);
                unsigned iB = CSGAddVertex(poly.vertices_[j-1], colocalTable, allVerts, receivingMesh);
                unsigned iC = CSGAddVertex(poly.vertices_[j], colocalTable, allVerts, receivingMesh);

                receivingMesh->indexBuffer_.push_back(iA);
                receivingMesh->indexBuffer_.push_back(iB);
                receivingMesh->indexBuffer_.push_back(iC);
                //model.vertices_.push_back(poly.vertices_[0]);		model.indices_.push_back(p++);
                //model.vertices_.push_back(poly.vertices_[j - 1]);	model.indices_.push_back(p++);
                //model.vertices_.push_back(poly.vertices_[j]);		model.indices_.push_back(p++);
            }
        }

        /*// T-Junction removal

        auto heMesh = receivingMesh->BuildHalfEdgeMesh();
        if (heMesh->splitBoundaryEdges())
        {
            receivingMesh->Clear();
            receivingMesh->positionBuffer_.resize(heMesh->vertexCount());
            receivingMesh->normalBuffer_.resize(heMesh->vertexCount());
            //if (includeUV)
                receivingMesh->uvBuffer_.resize(heMesh->vertexCount());
            receivingMesh->indexBuffer_.resize(heMesh->faceCount() * 3);

            for (unsigned i = 0; i < heMesh->vertexCount(); ++i)
            {
                auto vert = heMesh->vertexAt(i);
                receivingMesh->positionBuffer_[i] = vert->pos;
                receivingMesh->normalBuffer_[i] = vert->nor;
                //if (includeUV)
                    receivingMesh->uvBuffer_[i] = vert->tex;
            }

            unsigned startidx = 0;
            for (unsigned i = 0; i < heMesh->faceCount(); ++i)
            {
                auto face = heMesh->faceAt(i);
                auto edge = face->edge;
                uint32_t a = edge->vertex->id;
                uint32_t b = edge->next->vertex->id;
                uint32_t c = edge->prev->vertex->id;

                receivingMesh->indexBuffer_[startidx++] = a;
                receivingMesh->indexBuffer_[startidx++] = b;
                receivingMesh->indexBuffer_[startidx++] = c;
            }
        }
        delete heMesh;
        */
        receivingMesh->CalculateNormals();
#elif 0
        CSGModel model;
        int p = 0;
        for (size_t i = 0; i < polygons.size(); i++)
        {
            const CSGPolygon & poly = polygons[i];
            for (size_t j = 2; j < poly.vertices_.size(); j++)
            {
                model.vertices_.push_back(poly.vertices_[0]);		model.indices_.push_back(p++);
                model.vertices_.push_back(poly.vertices_[j - 1]);	model.indices_.push_back(p++);
                model.vertices_.push_back(poly.vertices_[j]);		model.indices_.push_back(p++);
            }
        }
        
        receivingMesh->Clear();
        receivingMesh->positionBuffer_.resize(model.vertices_.size());
        receivingMesh->normalBuffer_.resize(model.vertices_.size());
        if (includeUV)
            receivingMesh->uvBuffer_.resize(model.vertices_.size());
        receivingMesh->indexBuffer_.resize(model.indices_.size());

        for (unsigned i = 0; i < model.vertices_.size(); ++i)
        {
            receivingMesh->positionBuffer_[i] = model.vertices_[i].pos_;
            receivingMesh->normalBuffer_[i] = model.vertices_[i].normal_;
            if (includeUV)
                receivingMesh->uvBuffer_[i] = model.vertices_[i].uv_;
        }

        unsigned startidx = 0;
        for (unsigned i = 0; i < model.indices_.size(); i += 3)
        {
            receivingMesh->indexBuffer_[startidx++] = model.indices_[i];
            receivingMesh->indexBuffer_[startidx++] = model.indices_[i + 1];
            receivingMesh->indexBuffer_[startidx++] = model.indices_[i + 2];
        }
        receivingMesh->CalculateNormals();
#else
        nv::MeshBuilder meshBuilder;
        int p = 0;
        for (size_t i = 0; i < polygons.size(); i++)
        {
            const CSGPolygon & poly = polygons[i];
            for (size_t j = 2; j < poly.vertices_.size(); j++)
            {
#define WRITE_VERTEX_DATA(VERTEX) meshBuilder.addPosition(VERTEX.pos_); \
                meshBuilder.addNormal(VERTEX.normal_); \
                meshBuilder.addTexCoord(VERTEX.uv_)

                WRITE_VERTEX_DATA(poly.vertices_[0]);
                WRITE_VERTEX_DATA(poly.vertices_[j - 1]);
                WRITE_VERTEX_DATA(poly.vertices_[j]);

                meshBuilder.beginPolygon();
                meshBuilder.addVertex(p, p, p);
                meshBuilder.addVertex(p+1, p+1, p+1);
                meshBuilder.addVertex(p+2, p+2, p+2);
                //meshBuilder.addTriangle(p, p + 1, p + 2);
                if (meshBuilder.endPolygon())
                    p += 3;

                //model.vertices_.push_back(poly.vertices_[0]);		model.indices_.push_back(p++);
                //model.vertices_.push_back(poly.vertices_[j - 1]);	model.indices_.push_back(p++);
                //model.vertices_.push_back(poly.vertices_[j]);		model.indices_.push_back(p++);
            }
        }

        // eliminate redundancies
        meshBuilder.optimize();
        auto heMesh = meshBuilder.buildHalfEdgeMesh(true);
        if (heMesh->splitBoundaryEdges())
        {
            //heMesh = nv::unifyVertices(heMesh);
        }
        heMesh->triangulate();

        receivingMesh->Clear();
        receivingMesh->positionBuffer_.resize(heMesh->vertexCount());
        receivingMesh->normalBuffer_.resize(heMesh->vertexCount());
        if (includeUV)
            receivingMesh->uvBuffer_.resize(heMesh->vertexCount());
        receivingMesh->indexBuffer_.resize(heMesh->faceCount() * 3);

        for (unsigned i = 0; i < heMesh->vertexCount(); ++i)
        {
            auto vert = heMesh->vertexAt(i);
            receivingMesh->positionBuffer_[i] = vert->pos;
            receivingMesh->normalBuffer_[i] = vert->nor;
            if (includeUV)
                receivingMesh->uvBuffer_[i] = vert->tex;
        }

        unsigned startidx = 0;
        for (unsigned i = 0; i < heMesh->faceCount(); ++i)
        {
            auto face = heMesh->faceAt(i);
            auto edge = face->edge;
            uint32_t a = edge->vertex->id;
            uint32_t b = edge->next->vertex->id;
            uint32_t c = edge->next->next->vertex->id;

            receivingMesh->indexBuffer_[startidx++] = a;
            receivingMesh->indexBuffer_[startidx++] = b;
            receivingMesh->indexBuffer_[startidx++] = c;
        }
        delete heMesh;
#endif
        //receivingMesh->CalculateNormals();
    }

    void MeshMerger::CSGExtractModel(MeshData* receivingMesh, std::vector<CSGTriangle>& polygons) const
    {
        receivingMesh->Clear();
        int p = 0;
        for (unsigned i = 0; i < polygons.size(); ++i)
        {
            receivingMesh->positionBuffer_.push_back(polygons[i].triangle_.a);
            receivingMesh->positionBuffer_.push_back(polygons[i].triangle_.b);
            receivingMesh->positionBuffer_.push_back(polygons[i].triangle_.c);
            receivingMesh->indexBuffer_.push_back(p++);
            receivingMesh->indexBuffer_.push_back(p++);
            receivingMesh->indexBuffer_.push_back(p++);
        }
    }

    void MeshMerger::CSGUnion(MeshData* receiver, const MeshData* lhs, const MeshData* rhs, const Mat3x4& rhsTrans,  bool meshTogether)
    {
        std::vector<CSGPolygon> left, right;
        CSGPrepareModel(lhs, left, Mat3x4::identity, true);
        CSGPrepareModel(rhs, right, rhsTrans, true);

        std::vector<CSGPolygon> newLeft = left, newRight = right;
        auto rhsBounds = rhs->CalculateBounds();
        rhsBounds.TransformAsAABB(rhsTrans);
        auto bounds = lhs->CalculateBounds().Intersection(rhsBounds);
        if (bounds.IsDegenerate())
            return;

        SplitPolygons(left, newRight, bounds);//lhs->CalculateBounds());
        SplitPolygons(right, newLeft, bounds);//rhs->CalculateBounds());
        
        left = newLeft;
        right = newRight;
        
        EliminateByWinding(newLeft, rhs, rhsTrans, false, false);
        EliminateByWinding(newRight, lhs, Mat3x4::identity, false, false);

        if (meshTogether)
        {
            newLeft.insert(newLeft.end(), newRight.begin(), newRight.end());
            CSGExtractModel(receiver, newLeft, true);
        }
        else
        {

        }
    }

    void MeshMerger::CSGSubtract(MeshData* receiver, const MeshData* lhs, const MeshData* rhs, const Mat3x4& rhsTrans, bool meshTogether)
    {
        std::vector<CSGPolygon> left, right;
        CSGPrepareModel(lhs, left, Mat3x4::identity, true);
        CSGPrepareModel(rhs, right, rhsTrans, true);

        auto rhsBounds = rhs->CalculateBounds();
        rhsBounds.TransformAsAABB(rhsTrans);
        std::vector<CSGPolygon> newLeft = left, newRight = right;
        SplitPolygons(left, newRight, lhs->CalculateBounds());
        SplitPolygons(right, newLeft, rhsBounds);
        
        left = newLeft;
        right = newRight;
        
        EliminateByWinding(newLeft, rhs, rhsTrans, false, false);
        EliminateByWinding(newRight, lhs, Mat3x4::identity, true, true);

        if (meshTogether)
        {
            newLeft.insert(newLeft.end(), newRight.begin(), newRight.end());
            CSGExtractModel(receiver, newLeft, true);
        }
        else
        {

        }
    }

    void MeshMerger::CSGIntersect(MeshData* receiver, const MeshData* lhs, const MeshData* rhs, const Mat3x4& rhsTrans, bool meshTogether)
    {
        std::vector<CSGPolygon> left, right;
        CSGPrepareModel(lhs, left, Mat3x4::identity, true);
        CSGPrepareModel(rhs, right, rhsTrans, true);

        auto rhsBounds = rhs->CalculateBounds();
        rhsBounds.TransformAsAABB(rhsTrans);

        std::vector<CSGPolygon> newLeft = left, newRight = right;
        SplitPolygons(left, newRight, lhs->CalculateBounds());
        SplitPolygons(right, newLeft, rhsBounds);

        left = newLeft;
        right = newRight;

        EliminateByWinding(newLeft, rhs, rhsTrans, true, false);
        EliminateByWinding(newRight, lhs, Mat3x4::identity, true, false);

        if (meshTogether)
        {
            newLeft.insert(newLeft.end(), newRight.begin(), newRight.end());
            CSGExtractModel(receiver, newLeft, true);
        }
        else
        {

        }
    }

    void MeshMerger::ClipTo(MeshData* receiver, const MeshData* lhs, const MeshData* rhs, const Mat3x4& rhsTrans, bool meshTogether)
    {
        std::vector<CSGPolygon> left, right;
        CSGPrepareModel(lhs, left, Mat3x4::identity, true);
        CSGPrepareModel(rhs, right, rhsTrans, true);

        std::vector<CSGPolygon> newRight = right;
        SplitPolygons(left, newRight, lhs->CalculateBounds());

        right = newRight;

        EliminateByWinding(newRight, lhs, Mat3x4::identity, false, false);

        if (meshTogether)
        {
            CSGExtractModel(receiver, left, true);
            CSGExtractModel(receiver, newRight, true);
        }
        else
        {
            CSGExtractModel(receiver, newRight, true);
        }
    }

    void MeshMerger::RawUnion(std::vector<CSGPolygon>& lhs, std::vector<CSGPolygon>& rhs)
    {
        //SplitPolygons(lhs, rhs);
        EliminatePolygons(lhs, false);
        EliminatePolygons(rhs, false);
    }

    void MeshMerger::RawSubstract(std::vector<CSGPolygon>& lhs, std::vector<CSGPolygon>& rhs)
    {
        //SplitPolygons(lhs, rhs);
        EliminatePolygons(lhs, false);
        EliminatePolygons(rhs, true, true);
    }
    
    void MeshMerger::RawIntersect(std::vector<CSGPolygon>& lhs, std::vector<CSGPolygon>& rhs)
    {
        //SplitPolygons(lhs, rhs);
        EliminatePolygons(lhs, true);
        EliminatePolygons(rhs, true);
    }

    std::vector<CSGPolygon> MeshMerger::Select(std::vector<CSGPolygon>& list, BoundingBox bounds, bool erase)
    {
        std::vector<CSGPolygon> ret;
        ret.reserve(list.size());

        for (unsigned i = 0; i < list.size(); ++i)
        {
            auto& poly = list[i];
            if (poly.vertices_.size() != 3)
                continue;
            if (bounds.Intersects(math::Triangle(poly.vertices_[0].pos_, poly.vertices_[1].pos_, poly.vertices_[2].pos_)))
            {
                ret.push_back(poly);
                if (erase)
                {
                    list.erase(list.begin() + i);
                    --i;
                }
            }
        }

        return ret;
    }

    void MeshMerger::SplitTriangles(std::vector<CSGTriangle>& lhs, std::vector<CSGTriangle>& rhs)
    {
        for (unsigned leftIndex = 0; leftIndex < lhs.size(); ++leftIndex)
        {
            CSGTriangle& current = lhs[leftIndex];
            for (unsigned j = 0; j < rhs.size(); ++j)
            {
                // Don't allow a polygon to split a product more than once, triangle-to-triangle test is false alarm
                if (rhs[j].lastSplitter_ == leftIndex)
                    continue;
                math::Triangle a0, b0;
                math::Triangle a1, b1;
                Plane splitPlane = current.triangle_.PlaneCCW();
                int ret = splitPlane.Clip(rhs[j].triangle_, a0, b0);
                if (ret > 0)
                {
                    CSGTriangle aa0;
                    aa0.triangle_ = a0;
                    aa0.lastSplitter_ = leftIndex;
                    aa0.backSideMarked_ = 1;
                    rhs.push_back(aa0);
                    if (ret == 2)
                    {
                        CSGTriangle bb0;
                        bb0.backSideMarked_ = 1;
                        bb0.lastSplitter_ = leftIndex;
                        bb0.triangle_ = b0;
                        rhs.push_back(bb0);
                    }
                
                    splitPlane.ReverseNormal();
                    ret = splitPlane.Clip(rhs[j].triangle_, a1, b1);
                    if (ret > 0)
                    {
                        CSGTriangle aa1;
                        aa1.triangle_ = a1;
                        aa1.backSideMarked_ = -1;
                        aa1.lastSplitter_ = leftIndex;
                        rhs.push_back(aa1);
                        if (ret == 2)
                        {
                            CSGTriangle bb1;
                            bb1.backSideMarked_ = -1;
                            bb1.triangle_ = b1;
                            bb1.lastSplitter_ = leftIndex;
                            rhs.push_back(bb1);
                        }
                    }
                
                    // Remove the triangle that we have split
                    rhs.erase(rhs.begin() + j);
                    --j;
                }
            }
        }
    }

    /// This function is a bit odd since it handles both sides at once.
    void MeshMerger::SplitPolygons(std::vector<CSGPolygon>& lhs, std::vector<CSGPolygon>& rhs, const BoundingBox& bounds) const
    {
        for (unsigned i = 0; i < lhs.size(); ++i)
        {
            CSGPolygon& current = lhs[i];
            if (!current.ContainedBy(bounds))
                continue;
            for (unsigned j = 0; j < rhs.size(); ++j)
            {
                if (rhs[j].lastSplitter_ == i)
                    continue;
                if (!rhs[j].ContainedBy(bounds))
                    continue;
                if (!current.Intersects(rhs[j]))
                    continue;
                // Check for splitting the polygon
                if (SplitPolygon(current.plane_, rhs[j], rhs, i))
                {
                    //// If there was a split, than our top left triangle needs to split too
                    //if (SplitPolygon(rhs[j].plane_, current, lhs))
                    //{
                    //    // Remove and move iteration backwards one step
                    //    lhs.erase(lhs.begin() + i);
                    //    --i;
                    //}
                    // Remove and move iteration backwards one step
                    rhs.erase(rhs.begin() + j);
                    --j;
                }
            }
        }
    }

    bool MeshMerger::SplitPolygon(const Plane& plane, CSGPolygon& polygon, std::vector<CSGPolygon>& polygons, unsigned polyID) const
    {
        enum
        {
            COPLANAR = 0,
            FRONT = 1,
            BACK = 2,
            SPANNING = 3
        };

        // Classify each point as well as the entire polygon into one of the above
        // four classes.
        int polygonType = 0;
        bool anyCoplanar = false;
        std::vector<int> types;

        for (size_t i = 0; i < polygon.vertices_.size(); i++)
        {
            float t = plane.SignedDistance(polygon.vertices_[i].pos_); //plane.normal.Dot(polygon.vertices_[i].pos_) - plane.d;
            int type = (t < -CSG_EPSILON) ? BACK : ((t > CSG_EPSILON) ? FRONT : COPLANAR);
            polygonType |= type;
            if (type == COPLANAR)
                anyCoplanar = true;
            types.push_back(type);
        }

        // Put the polygon in the correct list, splitting it when necessary.
        switch (polygonType)
        {
            case COPLANAR:
                break;
            case FRONT:
                if (anyCoplanar)
                    polygon.backSideMarked_ = 1;
                break;
            case BACK:
                if (anyCoplanar)
                    polygon.backSideMarked_ = -1;
                break;
            case SPANNING:
            {
                std::vector<CSGVertex> f, b;
                for (size_t i = 0; i < polygon.vertices_.size(); i++)
                {
                    int j = (i + 1) % polygon.vertices_.size();
                    int ti = types[i], tj = types[j];
                    CSGVertex vi = polygon.vertices_[i], vj = polygon.vertices_[j];
                    if (ti != BACK) 
                        f.push_back(vi);
                    if (ti != FRONT) 
                        b.push_back(vi);
                    if ((ti | tj) == SPANNING)
                    {
                        float t = (plane.d - plane.normal.Dot(vi.pos_)) / plane.normal.Dot(vj.pos_ - vi.pos_);
                        
                        // New vertices don't need the safe checks
                        CSGVertex v = Interpolate(vi, vj, t);
                        f.push_back(v);

                        b.push_back(v);
                    }
                }
                if (f.size() >= 3) 
                {
                    polygons.push_back(CSGPolygon(f));
                    auto& newPoly = polygons.back();
                    newPoly.lastSplitter_ = polyID;
                    newPoly.backSideMarked_ = 1;
                }
                if (b.size() >= 3) 
                {
                    polygons.push_back(CSGPolygon(b));
                    auto& newPoly = polygons.back();
                    newPoly.lastSplitter_ = polyID;
                    newPoly.backSideMarked_ = -1;
                }
                return true;
            }
        }
        return false;
    }

    void MeshMerger::EliminatePolygons(std::vector<CSGPolygon>& polygons, bool inside, bool flipPassing) const
    {
        // Fill the initial set
        std::vector<char> polyMarks(polygons.size(), 0);
        for (unsigned i = 0; i < polygons.size(); ++i)
            polyMarks[i] = polygons[i].backSideMarked_;

#if 0
        // By building a half edge mesh it is possible to complete all polygons in a single sweep
        // The half-edge is used for colocality of a vertices and edges
        if (auto halfEdgeMesh = CSGPolygonsToHalfEdgeMesh(polygons))
        {
            // Visit every face
            auto faceIt = halfEdgeMesh->faces();
            while (!faceIt.isDone())
            {
                const uint32_t thisID = faceIt.current()->id;
                char& thisMark = polyMarks[thisID];  // Our mark may update mid-way through iterating the neighbors

                // Visit every neighboring polygon
                auto neighborIt = faceIt.current()->neighbors();
                while (!neighborIt.isDone())
                {
                    if (neighborIt.current()) // can be null if a boundary edge
                    {
                        const uint32_t neighborId = neighborIt.current()->id;
                        if (polyMarks[neighborId] == 0 && thisMark != 0)        // If we're marked and the other isn't then mark it
                            polyMarks[neighborId] = thisMark;
                        else if (polyMarks[neighborId] != 0 && thisMark == 0)   // If we're not marked but the other is, then mark ourself
                            polyMarks[thisID] = thisMark = polyMarks[neighborId];
                    }

                    neighborIt.advance();
                }

                faceIt.advance();
            }
            delete halfEdgeMesh;
        }
#elif 0
        CSGLocality locality(polygons);
        for (unsigned i = 0; i < polygons.size(); ++i)
        {
            auto neighborIt = locality.Neighbors(i);
            char thisMark = polyMarks[i];   // mark isn't const because it may be updated part-way through the loop of neighbors, allowing hitting those other neighbors
            while (!neighborIt.IsDone())
            {
                if (neighborIt.Current())
                {
                    //TODO
                    const uint32_t neighborId = neighborIt.CurrentIndex();
                    if (neighborId != -1)
                    {
                        if (polyMarks[neighborId] == 0 && thisMark != 0)        // If we're marked and the other isn't then mark it
                            polyMarks[neighborId] = thisMark;
                        else if (polyMarks[neighborId] != 0 && thisMark == 0)   // If we're not marked but the other is, then mark ourself
                            polyMarks[i] = thisMark = polyMarks[neighborId];
                    }
                }

                neighborIt.Advance();
            }
        }
#else
        CSGLocality locality(polygons);
        
        int iterationCt = 0;
        do
        {
            for (unsigned i = 0; i < polygons.size(); ++i)
            {
                char& thisMark = polyMarks[i];
                if (thisMark != 0)
                    continue;
                int negCounts = 0;
                int posCounts = 0;
                if (polygons[i].vertices_.size() < 3)
                    continue;
                for (auto& vertex : polygons[i].vertices_)
                {
                    auto colocal = locality.colocalVertices.find(vertex.pos_);
                    for (auto idx : colocal->second)
                    {
                        if (idx != i)
                        {
                            if (polyMarks[idx] > 0)
                                ++posCounts;
                            else if (polyMarks[idx] < 0)
                                --posCounts;
                        }
                    }
                }
                if (posCounts > 0)
                    thisMark = 1;
                else if (posCounts < negCounts)
                    thisMark = -1;
            }
            ++iterationCt;
        } while ((std::find(polyMarks.begin(), polyMarks.end(), 0) != polyMarks.end() && iterationCt < 20) || iterationCt < 3);

#endif

        // Take only the polygons whose mark has the desired sign
        std::vector<CSGPolygon> outPolys;
        outPolys.reserve(polygons.size());

        const unsigned char passingMark = inside ? -1 : 1;
        for (unsigned i = 0; i < polygons.size(); ++i)
            if (polyMarks[i] == passingMark)
            {
                if (flipPassing)
                    polygons[i].Flip();
                outPolys.push_back(polygons[i]);
            }

        polygons = outPolys;
    }

    void MeshMerger::EliminateTriangles(std::vector<CSGTriangle>& triangles, bool inside, bool flipPassing) const
    {
        // Fill the initial set
        std::vector<char> polyMarks(triangles.size(), 0);
        for (unsigned i = 0; i < triangles.size(); ++i)
            polyMarks[i] = triangles[i].backSideMarked_;

        CSGLocality locality(triangles);
        int iterationCt = 0;
        while (std::find(polyMarks.begin(), polyMarks.end(), 0) != polyMarks.end() && iterationCt < 20)
        {
            for (unsigned i = 0; i < triangles.size(); ++i)
            {
                char& thisMark = polyMarks[i];
                for (unsigned v = 0; v < 3; ++v)
                {
                    auto colocal = locality.colocalVertices.find(triangles[i].triangle_.CornerPoint(v));
                    for (auto idx : colocal->second)
                    {
                        if (idx != i)
                        {
                            if (polyMarks[idx] == 0 && thisMark != 0)        // If we're marked and the other isn't then mark it
                                polyMarks[idx] = thisMark;
                            else if (polyMarks[idx] != 0 && thisMark == 0)   // If we're not marked but the other is, then mark ourself
                                polyMarks[i] = thisMark = polyMarks[idx];
                        }
                    }
                }
            }
            ++iterationCt;
        }

        // Take only the polygons whose mark has the desired sign
        std::vector<CSGTriangle> outPolys;
        outPolys.reserve(triangles.size());

        const unsigned char passingMark = inside ? -1 : 1;
        for (unsigned i = 0; i < triangles.size(); ++i)
            if (polyMarks[i] == passingMark)
            {
                if (flipPassing)
                    std::swap(triangles[i].triangle_.b, triangles[i].triangle_.c);
                outPolys.push_back(triangles[i]);
            }

        triangles = outPolys;
    }
}