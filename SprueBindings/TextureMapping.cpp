#include "Stdafx.h"

#include "TextureMapping.h"

#include <SprueEngine/Geometry/MeshData.h>
#include <SprueEngine/Texturing/RasterizerData.h>
#include <SprueEngine/Texturing/SprueTextureBaker.h>

namespace SprueBindings
{

    TextureMappingData^ TextureMapping::CalculateSampleMap(SprueBindings::MeshData^ meshData, Matrix transform, int width, int height)
    {
        if (meshData == nullptr)
            return nullptr;

        TextureMappingData^ ret = gcnew TextureMappingData();

        auto internalMesh = meshData->GetInternalMeshData();
        

        return ret;
    }

}