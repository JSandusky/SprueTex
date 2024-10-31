#pragma once

using namespace System;
using namespace Microsoft::Xna::Framework;

namespace SprueEngine
{
    class ComputeBuffer;
    class ComputeShader;
    class ComputeKernel;
    class ComputeDevice;
}

namespace SprueBindings
{

    [Flags]
    public enum class ComputeBufferFlags
    {
        CBS_Read = 1,
        CBS_Write = 1 << 1,
        CBS_Image2D = 1 << 2,
        CBS_Image3D = 1 << 3,
        CBS_RGB = 1 << 4,
        CBS_Grayscale = 1 << 5,
        CBS_RGBA = 1 << 6,
        CBS_FloatData = 1 << 7
    };

    public ref class ComputeBuffer
    {
    public:
        ComputeBuffer(SprueEngine::ComputeBuffer* buffer);
        ~ComputeBuffer();

        void SetData(array<unsigned char>^ values);
        void SetData(array<float>^ values);
        void SetData(array<Vector3>^ values);
        void SetData(array<Vector4>^ values);

        void ReadData(array<unsigned char>^ values);
        void ReadData(array<float>^ values);
        void ReadData(array<Vector3>^ values);
        void ReadData(array<Vector4>^ values);

    internal:
        SprueEngine::ComputeBuffer* buffer_ = 0x0;
    };

    public ref class ComputeKernel
    {
    public:
        ComputeKernel(SprueEngine::ComputeKernel* kernel);
        ~ComputeKernel();

        void Bind(ComputeBuffer^ buffer, int index);
        void SetArg(int arg, int index);
        void SetArg(float arg, int index);
        void SetArg(Vector2 arg, int index);
        void SetArg(Vector3 arg, int index);
        void SetArg(Vector4 arg, int index);

        void Execute(int x, int y, int z);

    private:
        SprueEngine::ComputeKernel* kernel_ = 0x0;
    };

    public ref class ComputeShader
    {
    public:
        ComputeShader(SprueEngine::ComputeShader*);
        ~ComputeShader();

        bool Compile(System::String^ source, System::String^ defines);
        bool Compile(System::Collections::Generic::List<System::String^>^ sources, System::String^ defines);
        ComputeKernel^ GetKernel(System::String^ name);

    private:
        SprueEngine::ComputeShader* shader_ = 0x0;
    };

    public ref class ComputeDevice
    {
    public:
        ComputeDevice();
        ~ComputeDevice();

        bool Initialize();

        void Finish();
        
        ComputeBuffer^ CreateBuffer(int size, ComputeBufferFlags type);
        ComputeShader^ CreateShader();

        SprueEngine::ComputeDevice* GetInternalDevice() { return device_; }

    private:
        SprueEngine::ComputeDevice* device_ = 0x0;
    };
}