#pragma once

#include <SprueEngine/ClassDef.h>

namespace SprueEngine
{
    #define MAX_BONE_WEIGHTS 4

    class DistanceField;
    class MeshData;
    class Skeleton;

    class HeatDiffusionSkinning
    {
        NOCOPYDEF(HeatDiffusionSkinning);
    public:
        HeatDiffusionSkinning();

        void CalculateWeights(MeshData*, Skeleton* skeleton) const;

    private:
        struct Voxel {
            unsigned char boneIndices_[MAX_BONE_WEIGHTS];
            float boneDistances_[MAX_BONE_WEIGHTS];
            float boneWeights_[MAX_BONE_WEIGHTS];
            bool solid_;
        };

        Voxel* voxels_;

        void ProcessSkinning();
        void BalanceWeights();
    };

}