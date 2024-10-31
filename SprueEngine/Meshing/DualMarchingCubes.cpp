#include "DualMarchingCubes.h"

#include <SprueEngine/Math/QEF.h>
#include <SprueEngine/ObjectPool.h>

namespace SprueEngine
{

    DualMarchingCubes::Cell* DualMarchingCubes::cells_ = 0x0;

    static ObjectPool<DualMarchingCubes::Edge, DefaultMemoryAllocator<DualMarchingCubes::Edge> > DMCEdgePool(64 * 64 * 64 * 3);

    #define QEF_ERROR 1e-6f
    #define QEF_SWEEPS 8

#include "DualMarchingCubesTables.inl"

#define INDEX(X, Y, Z) ToArrayIndex(X, Y, Z, 65, 65, 65)

    DualMarchingCubes::Cell::Cell()
    {
        memset(edges_, 0, sizeof(edges_));
        edges_[0] = DMCEdgePool.New();
        edges_[3] = DMCEdgePool.New();
        edges_[8] = DMCEdgePool.New();
    }

    DualMarchingCubes::Cell::~Cell()
    {
        // Delete the edges we own and then our edges list
        DMCEdgePool.Delete(edges_[0]);
        DMCEdgePool.Delete(edges_[3]);
        DMCEdgePool.Delete(edges_[8]);
    }


    DualMarchingCubes::~DualMarchingCubes()
    {
        //if (cells_)
        //    delete[] cells_;
    }

    void DualMarchingCubes::GenerateMesh(IMeshable* meshable, VertexBuffer& vertexBuffer, IndexBuffer& indexBuffer)
    {
        if (cells_ == 0x0)
            cells_ = new Cell[65 * 65 * 65];
        
        for (int z = 0; z < 65; ++z)
            for (int y = 0; y < 65; ++y)
                for (int x = 0; x < 65; ++x)
                    GenerateCells(meshable, x, y, z);

        for (int x = 0; x < 65; ++x)
            for (int y = 0; y < 65; ++y)
                for (int z = 0; z < 65; ++z)
                    GenerateEdges(x, y, z);

        for (int x = 0; x < 65 - 1; ++x)
            for (int y = 0; y < 65 - 1; ++y)
                for (int z = 0; z < 65 - 1; ++z)
                    Polygonize(meshable, vertexBuffer, x, y, z);

        for (int x = 0; x < 65 - 1; ++x)
            for (int y = 0; y < 65 - 1; ++y)
                for (int z = 0; z < 65 - 1; ++z)
                    GenerateIndices(indexBuffer, x, y, z);
    }

    void DualMarchingCubes::GenerateCells(IMeshable* meshable, int x, int y, int z) const
    {
        const int index = INDEX(x, y, z);
        Cell& cell = cells_[index];
        cell.valid_ = true;
        
        cell.vertexCount_ = 0;
        cell.vertices_[0].edges_.clear();
        cell.vertices_[1].edges_.clear();
        cell.vertices_[2].edges_.clear();
        cell.vertices_[3].edges_.clear();
        cell.vertices_[0].valid_ = false;
        cell.vertices_[1].valid_ = false;
        cell.vertices_[2].valid_ = false;
        cell.vertices_[3].valid_ = false;

        GenerateEdge(*cell.edges_[0], meshable, x, y, z, 0);
        GenerateEdge(*cell.edges_[3], meshable, x, y, z, 3);
        GenerateEdge(*cell.edges_[8], meshable, x, y, z, 8);
    }

    void DualMarchingCubes::GenerateEdge(Edge& edge, IMeshable* meshable, int x, int y, int z, int i) const
    {
        edge.index_ = i;
        edge.a_ = Vec3(x, y, z) + DMCCornerDeltas[DMCEdgePairs[i][0]];
        edge.b_ = Vec3(x, y, z) + DMCCornerDeltas[DMCEdgePairs[i][1]];
        edge.valueA_ = meshable->CalculateDensity(edge.a_);
        edge.valueB_ = meshable->CalculateDensity(edge.b_);
        edge.vertices_.clear();
    }

    void DualMarchingCubes::Polygonize(IMeshable* meshable, VertexBuffer& vertexBuffer, int x, int y, int z) const
    {
        const int index = INDEX(x, y, z);
        if (!cells_[index].valid_)
            return;

        int cube_index = 0;
        for (int i = 0; i < 8; i++)
        {
            if (meshable->CalculateDensity(Vec3(x, y, z) + DMCCornerDeltas[i]) < 0)
                cube_index |= 1 << i;
        }

        if (cube_index == 0 || cube_index == 255)
            return;

        cells_[index].vertexCount_ = DMCVerticesNumberTable[cube_index];

        int v_index = 0;
        cells_[index].vertices_[0].valid_ = true;
        for (int e = 0; e < 16 /*second dim of DMCEdgePairs*/; e++)
        {
            if (DMCEdgesTable[cube_index][e] == -2)
                break;
            if (DMCEdgesTable[cube_index][e] == -1)
            {
                v_index++;
                if (v_index < cells_[index].vertexCount_)
                    cells_[index].vertices_[v_index].valid_ = true;
                continue;
            }

            //Cells[x, y, z].Vertices[v_index].Index = v_index;
            cells_[index].vertices_[v_index].edges_.push_back(DMCEdgesTable[cube_index][e]);
            cells_[index].edges_[DMCEdgesTable[cube_index][e]]->vertices_.push_back(&cells_[index].vertices_[v_index]);
            //Cells[x, y, z].Edges[EdgesTable[cube_index, e]].Flipped = Cells[x, y, z].Edges[EdgesTable[cube_index, e]].ValueA < 0;
        }

        auto normalFunc = [](IMeshable* meshable, Vec3& rfNormal, const Vec3& f) {
            const float H = 0.001f;
            const float dx = meshable->CalculateDensity(f + Vec3(H, 0.f, 0.f)) - meshable->CalculateDensity(f - Vec3(H, 0.f, 0.f));
            const float dy = meshable->CalculateDensity(f + Vec3(0.f, H, 0.f)) - meshable->CalculateDensity(f - Vec3(0.f, H, 0.f));
            const float dz = meshable->CalculateDensity(f + Vec3(0.f, 0.f, H)) - meshable->CalculateDensity(f - Vec3(0.f, 0.f, H));

            rfNormal.Set(dx, dy, dz);
            rfNormal.Normalize();
        };

        auto intersectionFunc = [](IMeshable* meshable, Edge& edge) {
            float minValue = 100000.f;
            float t = 0.f;
            float currentT = 0.f;
            const int steps = 32;
            const float increment = 1.f / (float)steps;
            while (currentT <= 1.f)
            {
                const Vec3 p = edge.a_ + ((edge.b_ - edge.a_) * currentT);
                const float density = fabs(meshable->CalculateDensity(p));
                if (density < minValue)
                {
                    minValue = density;
                    t = currentT;
                }

                currentT += increment;
            }

            return edge.a_ + ((edge.b_ - edge.a_) * t);
        };

        for (unsigned char vIdx = 0; vIdx < cells_[index].vertexCount_; ++vIdx)
        {
            Vertex& tx = cells_[index].vertices_[vIdx];
            if (!tx.valid_) //for edges 241/243, which were originally marked as having 2 vertices...?
                continue;
            Vec3 point;

            if (!IsBlocky())
            {
                SprueEngine::QefSolver solver;

                for (int e_i = 0; e_i < tx.edges_.size(); e_i++)
                {
                    Edge& e = *cells_[index].edges_[tx.edges_[e_i]];
                    Vec3 norm;
                    Vec3 intersection = intersectionFunc(meshable, e);
                    point += intersection;
                    
                    normalFunc(meshable, norm, intersection);
                    solver.add(intersection, norm);
                }
            
                if (solver.getData().numPoints > 0) // If we can solve the QEF then use that
                {
                    solver.solve(point, QEF_ERROR, QEF_SWEEPS, QEF_ERROR);
                    if (point.x < x || point.x >(x + 1) || point.y < y || point.y >(y + 1) || point.z < z || point.z >(z + 1))                    
                    {
                        point.x = CLAMP(point.x, x, x + 1);
                        point.y = CLAMP(point.y, y, y + 1);
                        point.z = CLAMP(point.z, z, z + 1);
                        //point = solver.getMassPoint();
                    }
                }
                else if (tx.edges_.size() > 0) // Can't solve the QEF, but we have edges to average out
                    point /= (float)tx.edges_.size();
                else // Just go with blocky style
                    point = Vec3(x, y, z) + Vec3::one * 0.5f;
            }
            else
                point = Vec3(x, y, z) + Vec3::one * 0.5f;

            tx.position_ = point;
            Vec3 norm;
            normalFunc(meshable, norm, point);
            Vec3 c_v = norm * 0.5f + Vec3::one * 0.5f;
            c_v.Normalize();
            
            tx.index_ = vertexBuffer.size();
            MeshVertex pv(tx.position_, norm);
            vertexBuffer.push_back(pv);
        }
    }

    void DualMarchingCubes::GenerateEdges(int x, int y, int z) const
    {
        const int index = INDEX(x, y, z);
        cells_[index].edges_[1] = cells_[INDEX(x + 1, y, z)].edges_[3];
        cells_[index].edges_[2] = cells_[INDEX(x, y, z + 1)].edges_[0];

        cells_[index].edges_[4] = cells_[INDEX(x, y + 1, z)].edges_[0];
        cells_[index].edges_[5] = cells_[INDEX(x + 1, y + 1, z)].edges_[3];
        cells_[index].edges_[6] = cells_[INDEX(x, y + 1, z + 1)].edges_[0];
        cells_[index].edges_[7] = cells_[INDEX(x, y + 1, z)].edges_[3];

        cells_[index].edges_[9] = cells_[INDEX(x + 1, y, z)].edges_[8];
        cells_[index].edges_[10] = cells_[INDEX(x + 1, y, z + 1)].edges_[8];
        cells_[index].edges_[11] = cells_[INDEX(x, y, z + 1)].edges_[8];
    }

    void DualMarchingCubes::GenerateIndices(IndexBuffer& indexBuffer, int x, int y, int z) const
    {
        const int index = INDEX(x, y, z);
        if (!cells_[index].valid_)
            return;

        int edge_indexes[] = { 0, 3, 8 };
        for (int i = 0; i < 3; i++)
        {
            Edge& e = *cells_[index].edges_[edge_indexes[i]];
            bool flipped = e.valueA_ < 0;
            if (e.vertices_.size() > 3)
            {
                //if (!UseFlatShading)
                {
                    if (!flipped)
                    {
                        indexBuffer.push_back(e.vertices_[2]->index_);
                        indexBuffer.push_back(e.vertices_[0]->index_);
                        indexBuffer.push_back(e.vertices_[1]->index_);

                        indexBuffer.push_back(e.vertices_[1]->index_);
                        indexBuffer.push_back(e.vertices_[3]->index_);
                        indexBuffer.push_back(e.vertices_[2]->index_);
                    }
                    else
                    {
                        indexBuffer.push_back(e.vertices_[2]->index_);
                        indexBuffer.push_back(e.vertices_[1]->index_);
                        indexBuffer.push_back(e.vertices_[0]->index_);

                        indexBuffer.push_back(e.vertices_[1]->index_);
                        indexBuffer.push_back(e.vertices_[2]->index_);
                        indexBuffer.push_back(e.vertices_[3]->index_);
                    }
                }
                //else
                //{
                //    AddFlatTriangle(flipped, e.Vertices[0].Index, e.Vertices[1].Index, e.Vertices[2].Index, e.Vertices[3].Index);
                //}
            }
            else if (e.vertices_.size() == 3)
            {
                //if (!UseFlatShading)
                {
                    if (!flipped)
                    {
                        indexBuffer.push_back(e.vertices_[0]->index_);
                        indexBuffer.push_back(e.vertices_[1]->index_);
                        indexBuffer.push_back(e.vertices_[2]->index_);
                    }
                    else
                    {
                        indexBuffer.push_back(e.vertices_[1]->index_);
                        indexBuffer.push_back(e.vertices_[0]->index_);
                        indexBuffer.push_back(e.vertices_[2]->index_);
                    }
                }
                //else
                //{
                //    AddFlatTriangle(flipped, e.Vertices[0].Index, e.Vertices[1].Index, e.Vertices[2].Index);
                //}
            }
        }
    }

    void DualMarchingCubes::AddFlatTriangle(bool flipped, int* indexes, int indexCt) const
    {

    }
}