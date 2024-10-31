#include "ThermalBoneWeighting.h"

#include <SprueEngine/Geometry/MeshData.h>
#include <SprueEngine/Geometry/MeshOctree.h>

#include <SprueEngine/Libs/nvmesh/halfedge/Vertex.h>

namespace SprueEngine
{

    ThermalBoneWeighting::ThermalBoneWeighting(MeshData* meshData)
    {
        nv::HalfEdge::Mesh* mesh = meshData->BuildHalfEdgeMesh();
        if (mesh)
        {
            for (unsigned boneIndex = 0; boneIndex < 50; ++boneIndex) // replace with bone count
            {
                LineSegment seg = GetBoneSegment();
                float boneBaseWeight = 1.0f;
                for (unsigned vertIndex = 0; vertIndex < mesh->vertexCount(); ++vertIndex)
                {
                    nv::HalfEdge::Vertex* vert = mesh->vertexAt(boneIndex);
                    if (!vert->isFirstColocal())
                        continue;

                    const Vec3 ptOnLine = seg.ClosestPoint(vert->pos);
                    const Vec3 vertNor = vert->nor;

                    Vec3 dirToLine = (vert->pos - ptOnLine).Normalized();
                    Vec3 norDirToLine = dirToLine.Normalized();
                    float dotVal = vert->nor.Dot(dirToLine);
                    if (dotVal < 0.0f)
                        continue;
                    
                    const float weight = (dirToLine.LengthSq() / (0.5f * (dotVal + 1.001f))) * boneBaseWeight;
                    ApplyBoneWeights(meshData, vert, vertIndex, weight, boneIndex);
                }

                for (unsigned vertIndex = 0; vertIndex < mesh->vertexCount(); ++vertIndex)
                {
                    nv::HalfEdge::Vertex* vert = mesh->vertexAt(boneIndex);
                    if (!vert->isFirstColocal())
                        continue;
                    
                }
            }

            delete mesh;
        }
    }

    LineSegment ThermalBoneWeighting::GetBoneSegment() const
    {
        return LineSegment();
    }

    void ThermalBoneWeighting::ApplyBoneWeights(MeshData* meshData, nv::HalfEdge::Vertex* vert, unsigned index, float weight, unsigned boneIndex)
    {
        // raycast

        IntVec4 boneIndices = meshData->boneIndices_[index];
        Vec4 boneWeights = meshData->boneWeights_[index];

        int chosenIndex = -1;
        bool handled = false;
        float minVal = 10000.0f;
        for (int i = 0; i < 4; ++i)
        {
            float existingWeight = boneWeights[i];
            minVal = SprueMin(minVal, existingWeight);
            if (existingWeight == 0.0f)
            {
                handled = true;
                chosenIndex = i;
                break;
            }
        }

        if (chosenIndex != -1)
        {
            for (int i = 0; i < 4; ++i)
            {
                if (boneWeights[i] == minVal)
                {
                    chosenIndex = i;
                    break;
                }
            }
        }

        if (chosenIndex != -1)
        {
            boneIndices[chosenIndex] = boneIndex;
            boneWeights[chosenIndex] = weight;
        }
        else
            return;

        auto colocals = vert->colocals();
        while (!colocals.isDone())
        {
            const unsigned localIndex = colocals.current()->id;

            meshData->boneIndices_[index] = boneIndices;
            meshData->boneWeights_[index] = boneWeights;

            colocals.advance();
        }
    }

}