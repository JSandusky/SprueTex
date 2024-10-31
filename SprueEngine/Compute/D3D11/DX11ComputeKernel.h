#pragma once

#include <SprueEngine/ClassDef.h>
#include <SprueEngine/Compute/ComputeKernel.h>

#include <d3d11.h>

namespace SprueEngine
{
    class DX11ComputeDevice;

    /// For DX11 compute kernel's don't actually make sense, but for interface compliance they're non-authoritative holders of the source shader with utility functions.
    class SPRUE DX11ComputeKernel : public ComputeKernel
    {
        friend class DX11ComputeShader;
        NOCOPYDEF(DX11ComputeKernel);
    public:
        /// Construct.
        DX11ComputeKernel(const std::string& name, ComputeDevice* device, ID3D11ComputeShader* shader);
        /// Destruct.
        virtual ~DX11ComputeKernel();

        /// Attach a buffer for input/output.
        virtual void Bind(ComputeBuffer* buffer, unsigned index);
        /// Execute the kernel for XYZ threads.
        virtual void Execute(unsigned x, unsigned y, unsigned z);
        /// Set a simple function argument.
        virtual void SetArg(unsigned index, void* value, unsigned sz);
        /// Tests whether it is possible to execute this kernel.
        virtual bool IsExecutable() const { return shader_ != 0x0 && device_ != 0x0; }

    protected:
        /// Same as that found in the source DX11ComputeShader.
        ID3D11ComputeShader* shader_ = 0x0;
    };

}