#pragma once

#include <SprueEngine/ClassDef.h>

namespace SprueEngine
{

class MeshData;
struct MeshOctree;

class SPRUE MeshDistanceField
{
    NOCOPYDEF(MeshDistanceField);
public:
    MeshDistanceField(MeshData* meshData, MeshOctree* meshOctree, unsigned desiredDepth);
    MeshDistanceField(MeshData* meshData, unsigned desiredDepth);
    virtual ~MeshDistanceField();

private:
    class DistanceFieldOctree;
    DistanceFieldOctree* rootNode_;
};

}