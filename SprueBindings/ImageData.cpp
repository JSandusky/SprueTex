#include "Stdafx.h"
#include "ImageData.h"

#include "Macros.h"

#include <SprueEngine/BlockMap.h>
#include <SprueEngine/Resource.h>
#include <SprueEngine/Loaders/BasicImageLoader.h>
#include <SprueEngine/Loaders/SVGLoader.h>

using namespace System::Runtime::InteropServices;

namespace SprueBindings
{
    Color Mul(Color^ lhs, float val)
    {
        return Color(
            (unsigned char)(lhs->R * val),
            (unsigned char)(lhs->G * val), 
            (unsigned char)(lhs->B * val), 
            (unsigned char)(lhs->A * val));
    }

    Color Add(Color^ lhs, Color^ rhs)
    {
        return Color(
            (unsigned char)(lhs->R + rhs->R), 
            (unsigned char)(lhs->G + rhs->G), 
            (unsigned char)(lhs->B + rhs->B), 
            (unsigned char)(lhs->A + rhs->A));
    }

    ImageData::ImageData(int width, int height)
    {
        Width = width;
        Height = height;
        Pixels = gcnew array<Color>(width * height);
        for (int i = 0; i < width * height; ++i)
            Pixels[i] = Color::TransparentBlack;
    }

    void ImageData::SetPixel(int x, int y, Color color)
    {
        x = Clamp(x, 0, Width - 1);
        y = Clamp(y, 0, Height - 1);
        int idx = (y * Width) + x;
        Pixels[idx] = color;
    }

    Color ImageData::GetPixel(int x, int y)
    {
        x = Clamp(x, 0, Width - 1);
        y = Clamp(y, 0, Height - 1);
        int idx = (y * Width) + x;
        return Pixels[idx];
    }

    Color ImageData::GetPixelUnsafe(int x, int y)
    {
        return Pixels[(y *Width) + x];
    }

    static inline int Image_Data_FastFloor(float f) { return (f >= 0.0f ? (int)f : (int)f - 1); }
    Color ImageData::GetPixelBilinear(float x, float y)
    {
        x = x - Image_Data_FastFloor(x);
        y = y - Image_Data_FastFloor(y);
        x = Clamp(x * Width - 0.5f, 0.0f, (float)(Width - 1));
        y = Clamp(y * Height - 0.5f, 0.0f, (float)(Height - 1));

        int xI = (int)x;
        int yI = (int)y;

        float xF = x - Image_Data_FastFloor(x);
        float yF = y - Image_Data_FastFloor(y);

        Color topValue = Add(Mul(GetPixel(xI, yI), (1.0f - xF)), Mul(GetPixel(xI + 1, yI), xF));
        Color bottomValue = Add(Mul(GetPixel(xI, yI + 1), (1.0f - xF)), Mul(GetPixel(xI + 1, yI + 1), xF));
        return Add(Mul(topValue, 1.0f - yF), Mul(bottomValue, yF));
    }

    Color ImageData::GetPixelBilinearX(float x, int y)
    {
        x = x - Image_Data_FastFloor(x);
        x = Clamp(x * Width - 0.5f, 0.0f, (float)(Width - 1));

        int xI = (int)x;
        float xF = x - Image_Data_FastFloor(x);

        return Add(Mul(GetPixel(xI, y), (1.0f - xF)), Mul(GetPixel(xI + 1, y), xF));
    }

    ImageData^ ImageData::Load(System::String^ file, PluginLib::IErrorPublisher^ errors)
    {
        if (file == nullptr)
        {
            errors->PublishError("Incorrect use of ImageData::Load", PluginLib::ErrorLevels::DEBUG);
            return nullptr;
        }

        if (!System::IO::File::Exists(file))
        {
            errors->PublishError(System::String::Format("Image does not exist: {0}", file), PluginLib::ErrorLevels::ERROR);
            return nullptr;
        }

        SprueEngine::BasicImageLoader loader;
        const char* path = STRING_TO_CHAR(file);
        if (auto loaded = loader.LoadResource(path))
        {
            auto image = std::dynamic_pointer_cast<SprueEngine::BitmapResource>(loaded);
            
            if (auto imageData = image->GetImage())
            {
                ImageData^ ret = gcnew ImageData();
                ret->Width = imageData->getWidth();
                ret->Height = imageData->getHeight();
                ret->Pixels = gcnew array<Color>(ret->Width * ret->Height);
                for (int y = 0; y < imageData->getHeight(); ++y)
                    for (int x = 0; x < imageData->getWidth(); ++x)
                    {
                        int index = imageData->toIndex(x, y);
                        auto pixelValue = imageData->get(x, y);
                        ret->Pixels[index].R = pixelValue.r * 255;
                        ret->Pixels[index].G = pixelValue.g * 255;
                        ret->Pixels[index].B = pixelValue.b * 255;
                        ret->Pixels[index].A = pixelValue.a * 255;
                    }
                return ret;
            }
            else
                errors->PublishError(System::String::Format("Corrupted data for image: {0}", System::IO::Path::GetFileName(file)), PluginLib::ErrorLevels::ERROR);
        }
        else
            errors->PublishError(System::String::Format("Unable to load image: {0}", System::IO::Path::GetFileName(file)), PluginLib::ErrorLevels::ERROR);

        return nullptr;
    }
    
    ImageData^ ImageData::LoadSVG(System::String^ file, int width, int height, PluginLib::IErrorPublisher^ errors)
    {
        if (file == nullptr)
        {
            errors->PublishError("Incorrect use of ImageData::LoadSVG", PluginLib::ErrorLevels::DEBUG);
            return nullptr;
        }

        if (!System::IO::File::Exists(file))
        {
            errors->PublishError(System::String::Format("SVG file does not exist: {0}", file), PluginLib::ErrorLevels::ERROR);
            return nullptr;
        }

        SprueEngine::SVGLoader loader;
        const char* path = STRING_TO_CHAR(file);
        if (!loader.CanLoad(path))
        {
            errors->PublishError(System::String::Format("Unable to load SVG file: {0}", file), PluginLib::ErrorLevels::ERROR);
            return nullptr;
        }

        if (auto loaded = loader.LoadResource(path))
        {
            auto svgRes = std::dynamic_pointer_cast<SprueEngine::SVGResource>(loaded);
            if (auto imageData = svgRes->GetImage(width, height))
            {
                ImageData^ ret = gcnew ImageData();
                ret->Width = imageData->getWidth();
                ret->Height = imageData->getHeight();
                ret->Pixels = gcnew array<Color>(ret->Width * ret->Height);
                for (int y = 0; y < imageData->getHeight(); ++y)
                    for (int x = 0; x < imageData->getWidth(); ++x)
                    {
                        int index = imageData->toIndex(x, y);
                        auto pixelValue = imageData->get(x, y);
                        ret->Pixels[index].R = pixelValue.r * 255;
                        ret->Pixels[index].G = pixelValue.g * 255;
                        ret->Pixels[index].B = pixelValue.b * 255;
                        ret->Pixels[index].A = pixelValue.a * 255;
                    }
                return ret;
            }
            else
                errors->PublishError(System::String::Format("Corrupted data for SVG image: {0}", System::IO::Path::GetFileName(file)), PluginLib::ErrorLevels::ERROR);
        }
        else
        {
            errors->PublishError(System::String::Format("Unable to load SVG file: {0}, bad resource data", file), PluginLib::ErrorLevels::ERROR);
            return nullptr;
        }
    }

    bool ImageData::Save(System::String^ file, ImageData^ image, PluginLib::IErrorPublisher^ errors)
    {
        if (image == nullptr || file == nullptr)
        {
            errors->PublishError("Incorrect use of ImageData::Save", PluginLib::ErrorLevels::DEBUG);
            return false;
        }

        SprueEngine::BasicImageLoader loader;
        if (image->Width > 0 && image->Height > 0)
        {
            SprueEngine::FilterableBlockMap<SprueEngine::RGBA> outData(image->Width, image->Height);
            
            for (int y = 0; y < outData.getHeight(); ++y)
                for (int x = 0; x < outData.getWidth(); ++x)
                {
                    unsigned index = outData.toIndex(x, y);
                    SprueEngine::RGBA pixel;
                    auto srcColor = image->Pixels[index];
                    pixel.r = ((float)srcColor.R) / 255.0f;
                    pixel.g = ((float)srcColor.G) / 255.0f;
                    pixel.b = ((float)srcColor.B) / 255.0f;
                    pixel.a = ((float)srcColor.A) / 255.0f;
                    outData.getData()[index] = pixel;
                }

            const char* stringName = STRING_TO_CHAR(file);
            if (file->EndsWith(".dds"))
            {
                loader.SaveDDS(&outData, stringName);
                return true;
            }
            else if (file->EndsWith(".png"))
            {
                loader.SavePNG(&outData, stringName);
                return true;
            }
            else if (file->EndsWith(".tga"))
            {
                loader.SaveTGA(&outData, stringName);
                return true;
            }
            else if (file->EndsWith(".hdr"))
            {
                loader.SaveHDR(&outData, stringName);
                return true;
            }

            errors->PublishError(System::String::Format("Unrecognized output format for image: {0}, format: {1}", file, System::IO::Path::GetExtension(file)), PluginLib::ErrorLevels::ERROR);
            return false;
        }
        else
            errors->PublishError(System::String::Format("Unable to save an 'empty' image to: {0}", System::IO::Path::GetFileName(file)), PluginLib::ErrorLevels::DEBUG);

        return false;
    }

    ImageData^ ImageData::GetResized(int newWidth, int newHeight)
    {
        if (newWidth == Width && newHeight == Height)
            return nullptr;
        if (newWidth == 0 || newHeight == 0)
            return nullptr;

        SprueEngine::FilterableBlockMap<SprueEngine::RGBA> outData(Width, Height);
        for (int y = 0; y < outData.getHeight(); ++y)
            for (int x = 0; x < outData.getWidth(); ++x)
            {
                unsigned index = outData.toIndex(x, y);
                SprueEngine::RGBA pixel;
                auto srcColor = Pixels[index];
                pixel.r = ((float)srcColor.R) / 255.0f;
                pixel.g = ((float)srcColor.G) / 255.0f;
                pixel.b = ((float)srcColor.B) / 255.0f;
                pixel.a = ((float)srcColor.A) / 255.0f;
                outData.getData()[index] = pixel;
            }

        //outData.resize(newWidth, newHeight);
        auto ret = gcnew ImageData();
        ret->Width = newWidth;
        ret->Height = newHeight;
        ret->Pixels = gcnew array<Color>(outData.getWidth() * outData.getHeight());
        for (int y = 0; y < ret->Width; ++y)
            for (int x = 0; x < ret->Height; ++x)
            {
                int index = outData.toIndex(x, y, 1, ret->Width, ret->Height, 1);
                auto pixelValue = outData.getBilinear(x / (float)(ret->Width-1), y / (float)(ret->Height-1));
                ret->Pixels[index].R = pixelValue.r * 255;
                ret->Pixels[index].G = pixelValue.g * 255;
                ret->Pixels[index].B = pixelValue.b * 255;
                ret->Pixels[index].A = pixelValue.a * 255;
            }

        return ret;
    }

    void ImageData::Resize(int newWidth, int newHeight)
    {
        if (newWidth == Width && newHeight == Height)
            return;
        if (newWidth == 0 || newHeight == 0)
            return;

        SprueEngine::FilterableBlockMap<SprueEngine::RGBA> outData(Width, Height);

        for (int y = 0; y < outData.getHeight(); ++y)
            for (int x = 0; x < outData.getWidth(); ++x)
            {
                unsigned index = outData.toIndex(x, y);
                SprueEngine::RGBA pixel;
                auto srcColor = Pixels[index];
                pixel.r = ((float)srcColor.R) / 255.0f;
                pixel.g = ((float)srcColor.G) / 255.0f;
                pixel.b = ((float)srcColor.B) / 255.0f;
                pixel.a = ((float)srcColor.A) / 255.0f;
                outData.getData()[index] = pixel;
            }

        outData.resize(newWidth, newHeight);
        Width = newWidth;
        Height = newHeight;

        Pixels = gcnew array<Color>(outData.getWidth() * outData.getHeight());
        for (int y = 0; y < Width; ++y)
            for (int x = 0; x < Height; ++x)
            {
                int index = outData.toIndex(x, y);
                auto pixelValue = outData.get(x, y);
                Pixels[index].R = pixelValue.r * 255;
                Pixels[index].G = pixelValue.g * 255;
                Pixels[index].B = pixelValue.b * 255;
                Pixels[index].A = pixelValue.a * 255;
            }
    }

    array<System::Byte>^ ImageData::ToMemory(ImageData^ image, PluginLib::IErrorPublisher^ errors)
    {
        if (image == nullptr)
            return nullptr;

        SprueEngine::BasicImageLoader loader;
        if (image->Width > 0 && image->Height > 0)
        {
            SprueEngine::FilterableBlockMap<SprueEngine::RGBA> outData(image->Width, image->Height);

            for (int y = 0; y < outData.getHeight(); ++y)
                for (int x = 0; x < outData.getWidth(); ++x)
                {
                    unsigned index = outData.toIndex(x, y);
                    SprueEngine::RGBA pixel;
                    auto srcColor = image->Pixels[index];
                    pixel.r = ((float)srcColor.R) / 255.0f;
                    pixel.g = ((float)srcColor.G) / 255.0f;
                    pixel.b = ((float)srcColor.B) / 255.0f;
                    pixel.a = ((float)srcColor.A) / 255.0f;
                    outData.getData()[index] = pixel;
                }

            SprueEngine::VectorBuffer dataBuffer;
            loader.SavePNG(&outData, dataBuffer);
            if (dataBuffer.GetSize() > 0)
            {
                array<System::Byte>^ ret = gcnew array<System::Byte>(dataBuffer.GetBuffer().size());
                pin_ptr<System::Byte> ptr = &ret[0];
                memcpy(ptr, dataBuffer.GetData(), dataBuffer.GetBuffer().size());
                return ret;
                //return gcnew System::String((const char*)dataBuffer.GetData(), 0, dataBuffer.GetBuffer().size(), System::Text::Encoding::ASCII);
            }
            errors->Warning("Unable to write PNG image to base64 encoding");
        }
        return nullptr;
    }

    ImageData^ ImageData::FromMemory(array<System::Byte>^ data, PluginLib::IErrorPublisher^ errors)
    {
        if (data == nullptr)
            return nullptr;
        if (data->Length == 0)
            return nullptr;

        SprueEngine::BasicImageLoader loader;
        SprueEngine::FilterableBlockMap<SprueEngine::RGBA> imageData;

        pin_ptr<System::Byte> dataString = &data[0];
        if (loader.ReadPNGMemory(&imageData, (const char*)dataString, data->Length))
        {
            ImageData^ ret = gcnew ImageData();
            ret->Width = imageData.getWidth();
            ret->Height = imageData.getHeight();
            ret->Pixels = gcnew array<Color>(ret->Width * ret->Height);
            for (int y = 0; y < imageData.getHeight(); ++y)
                for (int x = 0; x < imageData.getWidth(); ++x)
                {
                    int index = imageData.toIndex(x, y);
                    auto pixelValue = imageData.get(x, y);
                    ret->Pixels[index].R = pixelValue.r * 255;
                    ret->Pixels[index].G = pixelValue.g * 255;
                    ret->Pixels[index].B = pixelValue.b * 255;
                    ret->Pixels[index].A = pixelValue.a * 255;
                }
            return ret;
        }
        else
            errors->PublishError("Unable to read base64 image data", PluginLib::ErrorLevels::ERROR);


        return nullptr;
    }
}