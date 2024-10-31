#include "DensityFunctionCache.h"

#include <SprueEngine/Compute/ComputeDevice.h>
#include <SprueEngine/Compute/ComputeShader.h>
#include <SprueEngine/Compute/DensityShaderBuilder.h>
#include <SprueEngine/Core/SprueModel.h>

namespace SprueEngine
{

    DensityFunctionCache::DensityFunctionCache(ComputeDevice* device, DensityShaderBuilder* builder) :
        device_(device),
        builder_(builder)
    {
        SPRUE_ASSERT(device, "DensityFunctionCache was constructed without a ComputeDevice");
    }

    DensityFunctionCache::~DensityFunctionCache()
    {
        Clear();
    }

    std::pair<ComputeShader*, ComputeKernel*>* DensityFunctionCache::CreateNewData(SprueModel* forModel)
    {
        std::string code = builder_->BuildShader(forModel);
        if (code.empty())
            return 0x0;


        ComputeShader* newShader = device_->CreateShader(kernelName_);
        std::vector<std::string> srcs;
        srcs.reserve(preSources_.size() + postSources_.size() + 1);

        srcs.insert(srcs.end(), preSources_.begin(), preSources_.end());
        srcs.push_back(code);
        srcs.insert(srcs.end(), postSources_.begin(), postSources_.end());

        std::pair<ComputeShader*, ComputeKernel*>* data = 0x0;
        if (!newShader->CompileShader(srcs))
        {
            delete newShader;
            newShader = 0x0;
        }
        else
        {
            if (ComputeKernel* newkernel = newShader->GetKernel(kernelName_))
            {
                data = new std::pair<ComputeShader*, ComputeKernel*>(newShader, newkernel);
            }
            else
                delete newShader;
        }

        return data;
    }

    void DensityFunctionCache::DestroyData(std::pair<ComputeShader*, ComputeKernel*>* data)
    {
        if (data)
        {
            if (data->second)
                delete data->second;
            if (data->first)
                delete data->first;
        }
    }
}