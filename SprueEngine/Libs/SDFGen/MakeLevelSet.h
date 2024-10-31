#pragma once

#include <SprueEngine/BlockMap.h>
#include <SprueEngine/MathGeoLib/AllMath.h>

#include <vector>

namespace SprueEngine
{

    class MeshData;

    SPRUE FilterableBlockMap<float>* MakeLevelSet(MeshData* meshes,
        const Vec3& origin, float dx, int nx, int ny, int nz, const int exact_band = 1);

    SPRUE void CombineLevelSet(FilterableBlockMap<float>* targetSet, FilterableBlockMap<float>* sourceSet);

    SPRUE BlockMap<bool>* MakeSolidSet(FilterableBlockMap<float>* levelset, float max);

    SPRUE void MakeSolidSet(BlockMap<bool>* targetSolid, FilterableBlockMap<float>* levelset, float max);
}