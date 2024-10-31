#pragma once

#include <SprueEngine/BlockMap.h>

namespace SprueEngine
{

    class ComputeDevice;
    class Joint;
    class MeshData;

    class SPRUE HeatDiffusionSkinning
    {
    public:
        HeatDiffusionSkinning(BlockMap<bool>* solidMap);
        ~HeatDiffusionSkinning();

        bool GenerateHeatMap(ComputeDevice* device, Joint* joint);
        bool CalculateBoneWeights(MeshData* mesh);

    private:
        BlockMap<bool>* solidMap_ = 0x0;
        FilterableBlockMap<float>* heatMap_ = 0x0;
    };

}