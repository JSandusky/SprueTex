#include "DX11ComputeKernel.h"

#include "DX11ComputeDevice.h"
#include "DX11ComputeBuffer.h"

#include <d3d11.h>

namespace SprueEngine
{

    DX11ComputeKernel::DX11ComputeKernel(const std::string& name, ComputeDevice* device, ID3D11ComputeShader* shader) :
        ComputeKernel(name, device), 
        shader_(shader)
    {

    }

    DX11ComputeKernel::~DX11ComputeKernel()
    {

    }

    void DX11ComputeKernel::Bind(ComputeBuffer* buffer, unsigned index)
    {
        buffer->Bind(this, index);
    }

    void DX11ComputeKernel::Execute(unsigned x, unsigned y, unsigned z)
    {
        if (!shader_)
            return;
        ID3D11DeviceContext* context = ((DX11ComputeDevice*)device_)->GetContext();
        context->CSSetShader(shader_, 0x0, 0);
        context->Dispatch(x, y, z);
    }

    void DX11ComputeKernel::SetArg(unsigned index, void* value, unsigned sz)
    {

    }

}