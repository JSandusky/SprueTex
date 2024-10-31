#include "DX11ComputeDevice.h"

#include "DX11ComputeBuffer.h"
#include "DX11ComputeShader.h"

#include <d3d11.h>

namespace SprueEngine
{

    DX11ComputeDevice::DX11ComputeDevice(ID3D11Device* device, ID3D11DeviceContext* context) :
        d3dDevice_(device), context_(context)
    {
        
    }

    DX11ComputeDevice::~DX11ComputeDevice()
    {

    }

    bool DX11ComputeDevice::Initialize()
    {
        return false;
    }

    bool DX11ComputeDevice::IsValid() const
    {
        return false;
    }

    void DX11ComputeDevice::Finish()
    {

    }

    void DX11ComputeDevice::Barrier()
    {

    }
    
    ComputeBuffer* DX11ComputeDevice::CreateBuffer(const std::string& name, unsigned size, unsigned bufferType)
    {
        return new DX11ComputeBuffer(name, this, size, bufferType);
    }

    ComputeBuffer* DX11ComputeDevice::CreateBuffer(const std::string& name, unsigned width, unsigned height, unsigned bufferType)
    {
        return new DX11ComputeBuffer(name, this, width, height, bufferType);
    }

    ComputeBuffer* DX11ComputeDevice::CreateBuffer(const std::string& name, unsigned width, unsigned height, unsigned depth, unsigned bufferType)
    {
        return new DX11ComputeBuffer(name, this, width, height, depth, bufferType);
    }

    ComputeShader* DX11ComputeDevice::CreateShader(const std::string& name)
    {
        return new DX11ComputeShader(name, this);
    }
}