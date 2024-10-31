#include "MeshDistanceField.h"

#include <SprueEngine/Geometry/MeshData.h>
#include <SprueEngine/Geometry/MeshOctree.h>

namespace SprueEngine
{

class MeshDistanceField::DistanceFieldOctree
{
public:
    DistanceFieldOctree() : 
        children_(0x0)
    {

    }
    ~DistanceFieldOctree()
    {
        if (children_)
            delete[] children_;
    }

    void Split()
    {
        if (children_)
            return;
        children_ = new DistanceFieldOctree[8];

        const Vec3 center = cellStart_ + (cellSize_ / 2.0f);
        const Vec3 halfBounds = center - cellStart_;
        const Vec3 boundsMin = cellStart_;

        for (unsigned i = 0; i < 8; ++i)
        {
            children_[i].cellStart_ = cellStart_;
            if (i & 1)
                children_[i].cellStart_.x = center.x;
            if (i & 2)
                children_[i].cellStart_.y = center.y;
            if (i & 4)
                children_[i].cellStart_.z = center.z;

            children_[i].depth_ = depth_ + 1;
            children_[i].maxDepth_ = maxDepth_;
        }
    }

    DistanceFieldOctree* children_;
    Vec3 cellStart_;
    float cellSize_;
    unsigned char depth_;
    unsigned char maxDepth_;
};


MeshDistanceField::MeshDistanceField(MeshData* meshData, MeshOctree* meshOctree, unsigned desiredDepth)
{

}

MeshDistanceField::MeshDistanceField(MeshData* meshData, unsigned desiredDepth)
{

}

MeshDistanceField::~MeshDistanceField()
{
    if (rootNode_)
        delete rootNode_;
}

}