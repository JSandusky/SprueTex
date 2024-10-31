#pragma once

#include <SprueEngine/ClassDef.h>
#include <SprueEngine/MathGeoLib/AllMath.h>

namespace SprueEngine
{

    struct SPRUE BMeshBall
    {
        Vec3 position_;
        float radius_;
        float radiusX_;
        float radiusY_;
        int type_;
    };

    struct SPRUE BMeshLink
    {
        int start_;
        int end_;
    };

    class MeshData;

    MeshData* BuildBMesh(std::vector<BMeshBall> balls, std::vector<BMeshLink> links, int subdivisions);
}