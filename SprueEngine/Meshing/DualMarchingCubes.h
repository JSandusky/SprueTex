#pragma once

#include <SprueEngine/Meshing/MeshingAlgorithm.h>

namespace SprueEngine
{

    class SPRUE DualMarchingCubes : public MeshingAlgorithm
    {
        BASECLASSDEF(DualMarchingCubes, MeshingAlgorithm);
        NOCOPYDEF(DualMarchingCubes);
    public:
        DualMarchingCubes() { }
        virtual ~DualMarchingCubes();

        virtual void GenerateMesh(IMeshable* meshable, VertexBuffer& vertexBuffer, IndexBuffer& indexBuffer) override;

        bool IsBlocky() const { return blocky_; }
        void SetBlocky(bool blocky) { blocky_ = blocky; }

        struct Vertex
        {
            Vec3 position_;
            int index_;
            std::vector<int> edges_;
            bool valid_;
        };

        struct Edge
        {
            int index_;
            Vec3 a_;       // wasteful
            Vec3 b_;       // wasteful
            float valueA_; // wasteful
            float valueB_; // wasteful
            bool flipped_;
            std::vector<Vertex*> vertices_;
        };

    private:
        bool blocky_ = false;

        struct Cell
        {
            Vertex vertices_[4]; // Never more than 4 vertices
            Edge* edges_[12];
            bool valid_ = false;
            unsigned char vertexCount_ = 0;

            Cell();
            ~Cell();
        };
        static Cell* cells_;

        void GenerateCells(IMeshable* meshable, int x, int y, int z) const;
        void GenerateEdge(Edge& edge, IMeshable* meshable, int x, int y, int z, int i) const;
        void Polygonize(IMeshable* meshable, VertexBuffer&, int x, int y, int z) const;
        void GenerateEdges(int x, int y, int z) const;
        void GenerateIndices(IndexBuffer&, int x, int y, int z) const;
        void AddFlatTriangle(bool flipped, int* indexes, int indexCt) const;
    };

}