#pragma once

#include <SprueBindings/Compute.h>
#include <SprueBindings/Geometry.h>

#include <SprueEngine/BlockMap.h>

namespace SprueBindings
{
    public ref class LevelSetVoxelizer {
    public:
        LevelSetVoxelizer();
        ~LevelSetVoxelizer();

        bool MakeLevelSet(SprueBindings::ModelData^ model);
        bool MakeLevelSet(SprueBindings::MeshData^ meshData);
        bool Voxelize(SprueBindings::ModelData^ modelData, float max);
        bool Voxelize(SprueBindings::MeshData^ meshData, float max);

        // Voxelize accumulated level set
        bool VoxelizeLevelSet(float max);

        bool HasLevelSet() { return levelSet_ != 0x0; }
        SprueEngine::FilterableBlockMap<float>* GetLevelSet() { return levelSet_; }

    private:
        SprueEngine::FilterableBlockMap<float>* levelSet_ = 0x0;
        SprueEngine::BlockMap<bool>* byteMap_ = 0x0;
    };

    public ref class BoneWeightVoxelizer {
    public:
        BoneWeightVoxelizer();
        ~BoneWeightVoxelizer();

        bool CalculateBoneWeights(SprueBindings::ComputeDevice^ computeDevice);
    };

}