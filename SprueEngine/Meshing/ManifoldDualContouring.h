#pragma once

#include <SprueEngine/Meshing/MeshingAlgorithm.h>
#include <SprueEngine/ObjectPool.h>

namespace SprueEngine
{

    class SPRUE ManifoldDualContouring : public MeshingAlgorithm
    {
        BASECLASSDEF(ManifoldDualContouring, MeshingAlgorithm);
        NOCOPYDEF(ManifoldDualContouring);
    public:
        ManifoldDualContouring();
        virtual ~ManifoldDualContouring();

        virtual void GenerateMesh(IMeshable* meshable, VertexBuffer& vertexBuffer, IndexBuffer& indexBuffer) override;

        float GetThreshold() const { return threshold_; }
        void SetThreshold(float val) { threshold_ = val; }

    private:
        float threshold_;

        struct Vertex {
            Vertex* parent_;
            Vec3 position_;
            Vec3 normal_;
            int index_;
            int euler_;
            int cell_;
            bool faceTrait_;
            bool collapsible_;
            bool valid_;
        };

        struct Octree
        {
            Octree* children_[8];
            Vertex vertices_[8]; //how many maximum vertices?
            Vec3 position_;
            int index_;
            int childIndex_;
            int size_;
            unsigned char nodeType_;
            unsigned char corners_;

            void Construct(IMeshable*, int resolution, VertexBuffer&);
            void ClusterCells(IMeshable*);
            void GenerateVertices(VertexBuffer&);

        private:
            bool ConstructChildren(IMeshable* meshable, VertexBuffer& vertexBuffer, int& inlineIndex, int depth);
            bool ConstructLeaf(IMeshable* meshable, VertexBuffer& vertexBuffer, int& inlineIndex);
        };

        void GenerateIndices(IndexBuffer&);

        friend class ObjectPool < ManifoldDualContouring::Octree, DefaultMemoryAllocator<ManifoldDualContouring::Octree> >;

    public:
        typedef ObjectPool < ManifoldDualContouring::Octree, DefaultMemoryAllocator<ManifoldDualContouring::Octree> > OctreeNodePool;
    };

}