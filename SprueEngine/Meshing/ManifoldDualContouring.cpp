#include "ManifoldDualContouring.h"

namespace SprueEngine
{

    static ManifoldDualContouring::OctreeNodePool MDCOctreePool(65*65*65);

    enum ManifoldDCNodeType
    {
        MDC_None,
        MDC_Internal,
        MDC_Leaf,
        MDC_Collapsed
    };

#include "ManifoldDualContouringTables.inl"

    ManifoldDualContouring::ManifoldDualContouring()
    {

    }

    ManifoldDualContouring::~ManifoldDualContouring()
    {

    }

    void ManifoldDualContouring::GenerateMesh(IMeshable* meshable, VertexBuffer& vertexBuffer, IndexBuffer& indexBuffer)
    {
        Octree* tree = MDCOctreePool.New();

        tree->Construct(meshable, 64, vertexBuffer);
        tree->ClusterCells(meshable);
        tree->GenerateVertices(vertexBuffer);
        GenerateIndices(indexBuffer);

        MDCOctreePool.Delete(tree);
    }

    void ManifoldDualContouring::Octree::Construct(IMeshable* meshable, int resolution, VertexBuffer& vertexBuffer)
    {
        index_ = 0;
        position_ = Vec3::zero;
        size_ = resolution;
        nodeType_ = MDC_Internal;

        vertices_[0].valid_ = false;
        vertices_[1].valid_ = false;
        vertices_[2].valid_ = false;
        vertices_[3].valid_ = false;
        vertices_[4].valid_ = false;
        vertices_[5].valid_ = false;
        vertices_[6].valid_ = false;
        vertices_[7].valid_ = false;

        childIndex_ = 0;
        int n_index = 1;
        ConstructChildren(meshable, vertexBuffer, n_index, 1);

    }

    bool ManifoldDualContouring::Octree::ConstructChildren(IMeshable* meshable, VertexBuffer& vertexBuffer, int& inlineIndex, int depth)
    {
        if (size_ == 1)
            return ConstructLeaf(meshable, vertexBuffer, inlineIndex);

        nodeType_ = MDC_Internal;
        int childSize = size_ / 2;
        bool hasChildren = false;

        for (int i = 0; i < 8; ++i)
        {
            index_ = inlineIndex++;
            Vec3 childPos = MDCTCornerDeltas[i];
            children_[i] = MDCOctreePool.New();
            children_[i]->childIndex_ = i;

            if (children_[i]->ConstructChildren(meshable, vertexBuffer, inlineIndex, 0))
                hasChildren = true;
            else
            {
                MDCOctreePool.Delete(children_[i]);
                children_[i] = 0x0;
            }
        }
        return hasChildren;
    }

    bool ManifoldDualContouring::Octree::ConstructLeaf(IMeshable* meshable, VertexBuffer& vertexBuffer, int& inlineIndex)
    {
        if (size_ != 1)
            return false;

        index_ = inlineIndex++;
        nodeType_ = MDC_Leaf;

        unsigned char corners = 0;

        for (int i = 0; i < 8; ++i)
        {
            if (meshable->CalculateDensity(position_ + MDCTCornerDeltas[i]) <= 0.0f)
                corners |= 1 << i;
        }

        if (corners == 0 || corners == 255)
            return false;

        return true;
    }

    void ManifoldDualContouring::Octree::ClusterCells(IMeshable*)
    {

    }

    void ManifoldDualContouring::Octree::GenerateVertices(VertexBuffer& vertexBuffer)
    {

    }

    void ManifoldDualContouring::GenerateIndices(IndexBuffer& indexBuffer)
    {

    }
}