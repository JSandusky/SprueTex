#pragma once

using namespace System;
using namespace Microsoft::Xna::Framework;

namespace SprueBindings
{

    public ref class ImageData
    {
    public:
        array<Color>^ Pixels;
        int Width;
        int Height;
        void SetPixel(int x, int y, Color color);
        Color GetPixel(int x, int y);
        Color GetPixelUnsafe(int x, int y);
        Color GetPixelBilinear(float x, float y);
        Color GetPixelBilinearX(float x, int y);

        ImageData() {}
        ImageData(int width, int height);

        ImageData^ GetResized(int newWidth, int newHeight);
        void Resize(int newWidth, int newHeight);

        static array<System::Byte>^ ToMemory(ImageData^ image, PluginLib::IErrorPublisher^ errors);
        static ImageData^ FromMemory(array<System::Byte>^ data, PluginLib::IErrorPublisher^ errors);
        static ImageData^ Load(System::String^ file, PluginLib::IErrorPublisher^ errors);
        static ImageData^ LoadSVG(System::String^ file, int width, int height, PluginLib::IErrorPublisher^ errors);
        static bool Save(System::String^ file, ImageData^ image, PluginLib::IErrorPublisher^ errors);
    };

}