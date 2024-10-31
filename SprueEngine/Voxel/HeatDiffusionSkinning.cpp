#include "HeatDiffusionSkinning.h"

#include <SprueEngine/Geometry/MeshData.h>
#include <SprueEngine/Geometry/Skeleton.h>

namespace SprueEngine
{

    HeatDiffusionSkinning::HeatDiffusionSkinning(BlockMap<bool>* solidMap) :
        solidMap_(solidMap)
    {

    }
    HeatDiffusionSkinning::~HeatDiffusionSkinning()
    {
        if (heatMap_)
            delete heatMap_;
        heatMap_ = 0x0;
    }

    bool HeatDiffusionSkinning::GenerateHeatMap(ComputeDevice* device, Joint* joint)
    {
        return false;
    }

    bool HeatDiffusionSkinning::CalculateBoneWeights(MeshData* mesh)
    {
        return false;
    }

}