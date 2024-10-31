#pragma once

#include <SprueEngine/ClassDef.h>
#include <SprueEngine/MathGeoLib/Geometry/LineSegment.h>

#include <vector>

namespace nv
{
    namespace HalfEdge
    {
        struct Vertex;
    }
}

namespace SprueEngine
{
    class MeshData;

    /// Uses thermal bone weighting:
    /// See: "Automatic Rigging and Animation of 3D Characters" by Baran and Popovic. (2007)
    class SPRUE ThermalBoneWeighting
    {
    public:
        ThermalBoneWeighting(MeshData* meshData);

    private:
        LineSegment GetBoneSegment() const;
        void ApplyBoneWeights(MeshData* meshData, nv::HalfEdge::Vertex* vert, unsigned index, float weight, unsigned boneIndex);
    };

}