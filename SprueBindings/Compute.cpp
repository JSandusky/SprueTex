#include "Stdafx.h"
#include "Compute.h"

#include <SprueEngine/Compute/OpenCL/OpenCLComputeBuffer.h>
#include <SprueEngine/Compute/OpenCL/OpenCLComputeDevice.h>
#include <SprueEngine/Compute/OpenCL/OpenCLComputeKernel.h>
#include <SprueEngine/Compute/OpenCL/OpenCLComputeShader.h>

#include <SprueEngine/MathGeoLib/AllMath.h>

using namespace System::Runtime::InteropServices;

#define STRING_TO_CHAR(SRC) (const char*)(Marshal::StringToHGlobalAnsi(SRC)).ToPointer()

namespace SprueBindings
{

ComputeBuffer::ComputeBuffer(SprueEngine::ComputeBuffer* buffer)
{
    buffer_ = buffer;
}

ComputeBuffer::~ComputeBuffer()
{
    if (buffer_ != 0x0)
        delete buffer_;
}

void ComputeBuffer::SetData(array<unsigned char>^ values)
{
    {
        pin_ptr<unsigned char> data = &values[0];
        buffer_->SetData<unsigned char>((unsigned char*)data, values->Length);
    }
}

void ComputeBuffer::SetData(array<float>^ values)
{
    {
        pin_ptr<float> data = &values[0];
        buffer_->SetData<float>((float*)data, values->Length);
    }
}

void ComputeBuffer::SetData(array<Vector3>^ values)
{
    {
        pin_ptr<Vector3> data = &values[0];
        buffer_->SetData<SprueEngine::Vec3>((SprueEngine::Vec3*)data, values->Length);
    }
}

void ComputeBuffer::SetData(array<Vector4>^ values)
{
    {
        pin_ptr<Vector4> data = &values[0];
        buffer_->SetData<SprueEngine::Vec4>((SprueEngine::Vec4*)data, values->Length);
    }
}

void ComputeBuffer::ReadData(array<unsigned char>^ values)
{
    {
        pin_ptr<unsigned char> data = &values[0];
        buffer_->ReadData<unsigned char>((unsigned char*)data, values->Length);
    }
}

void ComputeBuffer::ReadData(array<float>^ values)
{
    {
        pin_ptr<float> data = &values[0];
        buffer_->ReadData<float>((float*)data, values->Length);
    }
}

void ComputeBuffer::ReadData(array<Vector3>^ values)
{
    {
        pin_ptr<Vector3> data = &values[0];
        buffer_->ReadData<SprueEngine::Vec3>((SprueEngine::Vec3*)data, values->Length);
    }
}

void ComputeBuffer::ReadData(array<Vector4>^ values)
{
    {
        pin_ptr<Vector4> data = &values[0];
        buffer_->ReadData<SprueEngine::Vec4>((SprueEngine::Vec4*)data, values->Length);
    }
}

ComputeKernel::ComputeKernel(SprueEngine::ComputeKernel* kernel)
{
    kernel_ = kernel;
}

ComputeKernel::~ComputeKernel()
{
    if (kernel_)
        delete kernel_;
}

void ComputeKernel::Bind(ComputeBuffer^ buffer, int index)
{
    kernel_->Bind(buffer->buffer_, index);
}

void ComputeKernel::SetArg(int arg, int index)
{
    kernel_->SetArg<int>(index, arg);
}

void ComputeKernel::SetArg(float arg, int index)
{
    kernel_->SetArg<float>(index, arg);
}

void ComputeKernel::SetArg(Vector2 arg, int index)
{
    SprueEngine::Vec2 v(arg.X, arg.Y);
    kernel_->SetArg<SprueEngine::Vec2>(index, v);
}

void ComputeKernel::SetArg(Vector3 arg, int index)
{
    SprueEngine::Vec3 v(arg.X, arg.Y, arg.Z);
    kernel_->SetArg<SprueEngine::Vec3>(index, v);
}

void ComputeKernel::SetArg(Vector4 arg, int index)
{
    SprueEngine::Vec4 v(arg.X, arg.Y, arg.Z, arg.W);
    kernel_->SetArg<SprueEngine::Vec4>(index, v);
}

void ComputeKernel::Execute(int x, int y, int z)
{
    kernel_->Execute(x, y, z);
}

ComputeShader::ComputeShader(SprueEngine::ComputeShader* shader)
{
    shader_ = shader;
}

ComputeShader::~ComputeShader()
{
    if (shader_)
    {
        delete shader_;
        shader_ = 0x0;
    }
}

bool ComputeShader::Compile(System::String^ source, System::String^ defines)
{
    return shader_->CompileShader(STRING_TO_CHAR(source), STRING_TO_CHAR(defines));
}

bool ComputeShader::Compile(System::Collections::Generic::List<System::String^>^ sources, System::String^ defines)
{
    std::vector<std::string> srcs;
    for (int i = 0; i < sources->Count; ++i)
        srcs.push_back(STRING_TO_CHAR((*sources)[i]));
    return shader_->CompileShader(srcs, STRING_TO_CHAR(defines));
}

ComputeKernel^ ComputeShader::GetKernel(System::String^ name)
{
    std::string nm = STRING_TO_CHAR(name);
    return gcnew ComputeKernel(shader_->GetKernel(nm));
}

ComputeDevice::ComputeDevice()
{
    device_ = new SprueEngine::OpenCLComputeDevice();
}

ComputeDevice::~ComputeDevice()
{
    if (device_ != 0x0)
        delete device_;
}

bool ComputeDevice::Initialize()
{
    return device_->Initialize();
}

void ComputeDevice::Finish()
{
    device_->Finish();
}

ComputeBuffer^ ComputeDevice::CreateBuffer(int size, ComputeBufferFlags type)
{
    return gcnew ComputeBuffer(device_->CreateBuffer("", (unsigned)size, (unsigned)type));
}

ComputeShader^ ComputeDevice::CreateShader()
{
    return gcnew ComputeShader(device_->CreateShader(""));
}

}