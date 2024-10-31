#include "Stdafx.h"
#include "Voxelization.h"

#include <SprueEngine/Compute/ComputeDevice.h>
#include <SprueEngine/Libs/SDFGen/MakeLevelSet.h>

namespace SprueBindings
{

    LevelSetVoxelizer::LevelSetVoxelizer()
    {

    }

    LevelSetVoxelizer::~LevelSetVoxelizer()
    {
        if (levelSet_ != nullptr)
            delete levelSet_;
        if (byteMap_ != nullptr)
            delete byteMap_;
    }

    bool LevelSetVoxelizer::MakeLevelSet(SprueBindings::ModelData^ model)
    {
        if (model == nullptr)
            return false;

        bool success = true;
        for (int i = 0; i < model->Meshes->Count; ++i)
        {
            auto mesh = model->Meshes[i];
            success &= MakeLevelSet(mesh);
        }

        return success;
    }

    bool LevelSetVoxelizer::MakeLevelSet(SprueBindings::MeshData^ meshData)
    {
        if (meshData == nullptr || meshData->GetInternalMeshData() == nullptr)
            return false;

        auto set = SprueEngine::MakeLevelSet(meshData->GetInternalMeshData(), SprueEngine::Vec3(0, 0, 0), 1, 64, 64, 64);
        if (set != nullptr)
        {
            if (levelSet_ != nullptr)
            {
                SprueEngine::CombineLevelSet(levelSet_, set);
                delete set;
            }
            else
                levelSet_ = set;
            return true;
        }
    }

    bool LevelSetVoxelizer::VoxelizeLevelSet(float max)
    {
        if (levelSet_ == nullptr)
            return false;
        if (byteMap_ != nullptr)
            delete byteMap_;

        byteMap_ = SprueEngine::MakeSolidSet(levelSet_, max);
    }

    bool LevelSetVoxelizer::Voxelize(SprueBindings::ModelData^ model, float max)
    {
        if (model == nullptr)
            return false;

        if (!MakeLevelSet(model))
            return false;

        return VoxelizeLevelSet(max);
    }

    bool LevelSetVoxelizer::Voxelize(SprueBindings::MeshData^ meshData, float max)
    {
        if (meshData == nullptr || meshData->GetInternalMeshData() == 0x0)
            return false;

        if (levelSet_ != nullptr)
            delete levelSet_;

        levelSet_ = SprueEngine::MakeLevelSet(meshData->GetInternalMeshData(), SprueEngine::Vec3(0, 0, 0), 1, 64, 64, 64);
        if (levelSet_ != nullptr)
            return VoxelizeLevelSet(max);

        return false;
    }

    BoneWeightVoxelizer::BoneWeightVoxelizer()
    {

    }

    BoneWeightVoxelizer::~BoneWeightVoxelizer()
    {

    }

    bool BoneWeightVoxelizer::CalculateBoneWeights(SprueBindings::ComputeDevice^ computeDevice)
    {
        if (computeDevice == nullptr)
            return false;

        auto device = computeDevice->GetInternalDevice();

        return false;
    }
}