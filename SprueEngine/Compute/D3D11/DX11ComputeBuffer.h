#pragma once

#include <SprueEngine/ClassDef.h>
#include <SprueEngine/Compute/ComputeBuffer.h>

namespace SprueEngine
{
    class DX11ComputeDevice;

    /// Manages a buffer of data to be used by DX11 compute shaders (or other things)
    /// May also be constructed from a ID3DBuffer object for external use. 
    class SPRUE DX11ComputeBuffer : public ComputeBuffer
    {
    public:
        /// Construct a data buffer.
        DX11ComputeBuffer(const std::string& name, DX11ComputeDevice*, unsigned size, unsigned settings);
        /// Construct a 2d image buffer.
        DX11ComputeBuffer(const std::string& name, DX11ComputeDevice* device, unsigned width, unsigned height, unsigned settings);
        /// Construct a 3d image buffer.
        DX11ComputeBuffer(const std::string& name, DX11ComputeDevice* device, unsigned width, unsigned height, unsigned depth, unsigned settings);
        /// Destruct and free resources.
        virtual ~DX11ComputeBuffer();

        /// Sets the data in the contained buffer.
        virtual void SetData(void* data, unsigned offset, unsigned len) override;
        /// Reads data from the buffer.
        virtual void ReadData(void* data, unsigned offset, unsigned len) override;
        /// Binds the buffer to the kernel at the specified register.
        virtual void Bind(ComputeKernel* kernel, unsigned index) override;
    };

}