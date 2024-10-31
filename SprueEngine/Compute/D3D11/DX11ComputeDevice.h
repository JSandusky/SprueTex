#pragma once

#include <SprueEngine/ClassDef.h>
#include <SprueEngine/Compute/ComputeDevice.h>

class ID3D11Device;
class ID3D11DeviceContext;

namespace SprueEngine
{

    /// Works with a DX device and device context for handling compute shaders.
    class SPRUE DX11ComputeDevice : public ComputeDevice
    {
        NOCOPYDEF(DX11ComputeDevice);
    public:
        /// Construct for an existing D3D11 instantiation.
        DX11ComputeDevice(ID3D11Device* device, ID3D11DeviceContext* context);
        /// Destruct.
        virtual ~DX11ComputeDevice();

        /// Initializes the device, returns true if successful.
        virtual bool Initialize() override;
        /// Returns true if the device is in a valid state for usage.
        virtual bool IsValid() const override;
        /// Blocks execution until all pending tasks are complete.
        virtual void Finish() override;
        virtual void Barrier() override;

        /// Construct an arbitrary data buffer.
        virtual ComputeBuffer* CreateBuffer(const std::string& name, unsigned size, unsigned bufferType) override;
        /// Construct an Image2D.
        virtual ComputeBuffer* CreateBuffer(const std::string& name, unsigned width, unsigned height, unsigned bufferType) override;
        /// Construct an Image3D.
        virtual ComputeBuffer* CreateBuffer(const std::string& name, unsigned width, unsigned height, unsigned depth, unsigned bufferType) override;
        /// Construct a shader (the shader is neither prepared nor compiled).
        virtual ComputeShader* CreateShader(const std::string& name) override;

        /// Returns the associated D3D11 device.
        ID3D11Device* GetD3DDevice() { return d3dDevice_; }
        /// Returns the associated D3D11 context.
        ID3D11DeviceContext* GetContext() { return context_; }

    protected:
        /// The D3D11 device that this compute 'device' is associated with.
        ID3D11Device* d3dDevice_ = 0x0;
        /// The D3D11 device context that this compute 'device' is associated with.
        ID3D11DeviceContext* context_ = 0x0;
    };

}