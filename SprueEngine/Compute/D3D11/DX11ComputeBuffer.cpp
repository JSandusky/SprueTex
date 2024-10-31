#include "DX11ComputeBuffer.h"

#include "DX11ComputeDevice.h"

namespace SprueEngine
{

    DX11ComputeBuffer::DX11ComputeBuffer(const std::string& name, DX11ComputeDevice* device, unsigned size, unsigned settings) :
        ComputeBuffer(name, device, size, settings)
    {

    }

    DX11ComputeBuffer::DX11ComputeBuffer(const std::string& name, DX11ComputeDevice* device, unsigned width, unsigned height, unsigned settings) :
        ComputeBuffer(name, device, width, height, settings)
    {

    }

    DX11ComputeBuffer::DX11ComputeBuffer(const std::string& name, DX11ComputeDevice* device, unsigned width, unsigned height, unsigned depth, unsigned settings) :
        ComputeBuffer(name, device, width, height, depth, settings)
    {

    }

    DX11ComputeBuffer::~DX11ComputeBuffer()
    {

    }

    void DX11ComputeBuffer::SetData(void* data, unsigned offset, unsigned len)
    {

    }

    void DX11ComputeBuffer::ReadData(void* data, unsigned offset, unsigned len)
    {

    }

    void DX11ComputeBuffer::Bind(ComputeKernel* kernel, unsigned index)
    {

    }
}