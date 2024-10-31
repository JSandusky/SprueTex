#pragma once

#include <SprueEngine/Meshing/StructuralHashCache.h>

namespace SprueEngine
{
    class DensityShaderBuilder;
    class ComputeDevice;
    class ComputeShader;
    class ComputeKernel;
    class SprueModel;

    /// Maintains a cache of compute shaders for calculating density. 
    /// The contained shaders are intended to be linked into kernels.
    class SPRUE DensityFunctionCache : public StructuralHashCache< std::pair<ComputeShader*, ComputeKernel*> >
    {
    public:
        /// Construct.
        DensityFunctionCache(ComputeDevice* device, DensityShaderBuilder* builder);
        /// Destruct.
        ~DensityFunctionCache();
        
        std::vector<std::string>& GetPreSources() { return preSources_; }
        std::vector<std::string>& GetPostSources() { return postSources_; }
        std::string GetKernelName() const { return kernelName_; }
        void SetKernelName(const std::string& name) { kernelName_ = name; }

    protected:
        virtual std::pair<ComputeShader*, ComputeKernel*>* CreateNewData(SprueModel* forModel) override;
        virtual void DestroyData(std::pair<ComputeShader*, ComputeKernel*>* data) override;

    private:
        ComputeDevice* device_;
        DensityShaderBuilder* builder_;
        std::vector<std::string> preSources_;
        std::vector<std::string> postSources_;
        std::string kernelName_;
    };

}