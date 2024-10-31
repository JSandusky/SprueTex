#include "Stdafx.h"

using namespace System;
using namespace System::Runtime::InteropServices;

#include "Bakers.h"

#include <SprueEngine/Texturing/TextureBakers.h>

#include <memory>

namespace SprueBindings
{

    ImageData^ Bakers::AmbientOcclusion(array<MeshData^>^ meshData, int width, int height, BakingProgressCallback^ callback)
    {
        SprueEngine::BAKER_CANCELATION cancelFunc = callback != nullptr ? static_cast<SprueEngine::BAKER_CANCELATION>(Marshal::GetFunctionPointerForDelegate(callback).ToPointer()) : 0x0;

        std::auto_ptr<SprueEngine::FilterableBlockMap<SprueEngine::RGBA>> blockmap(new SprueEngine::FilterableBlockMap<SprueEngine::RGBA>(width, height));

        for (int i = 0; i < meshData->Length; ++i)
        {
            auto mesh = meshData[i];
            auto actualMeshData = mesh->GetInternalMeshData();

            SprueEngine::AmbientOcclusionBaker baker(actualMeshData->BuildHalfEdgeMesh(), actualMeshData);
            baker.SetWidth(width);
            baker.SetHeight(height);
            baker.Bake(blockmap.get(), cancelFunc);
        }

        auto ret = gcnew ImageData();
        ret->Resize(width, height);
        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                auto pixel = blockmap->getData()[y * width + x];
                ret->Pixels[y * width + x] = Microsoft::Xna::Framework::Color(pixel.r, pixel.g, pixel.b, pixel.a);
            }
        }
        return ret;
    }

    ImageData^ Bakers::Thickness(array<MeshData^>^ meshData, int width, int height, BakingProgressCallback^ callback)
    {
        SprueEngine::BAKER_CANCELATION cancelFunc = callback != nullptr ? static_cast<SprueEngine::BAKER_CANCELATION>(Marshal::GetFunctionPointerForDelegate(callback).ToPointer()) : 0x0;

        std::auto_ptr<SprueEngine::FilterableBlockMap<SprueEngine::RGBA>> blockmap(new SprueEngine::FilterableBlockMap<SprueEngine::RGBA>(width, height));

        for (int i = 0; i < meshData->Length; ++i)
        {
            auto mesh = meshData[i];
            auto actualMeshData = mesh->GetInternalMeshData();

            SprueEngine::ThicknessBaker baker(actualMeshData->BuildHalfEdgeMesh(), actualMeshData);
            baker.SetWidth(width);
            baker.SetHeight(height);
            baker.Bake(blockmap.get(), cancelFunc);
        }

        auto ret = gcnew ImageData();
        ret->Resize(width, height);
        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                auto pixel = blockmap->getData()[y * width + x];
                ret->Pixels[y * width + x] = Microsoft::Xna::Framework::Color(pixel.r, pixel.g, pixel.b, pixel.a);
            }
        }
        return ret;
    }

    ImageData^ Bakers::Curvature(array<MeshData^>^ meshData, int width, int height, BakingProgressCallback^ callback)
    {
        SprueEngine::BAKER_CANCELATION cancelFunc = callback != nullptr ? static_cast<SprueEngine::BAKER_CANCELATION>(Marshal::GetFunctionPointerForDelegate(callback).ToPointer()) : 0x0;

        std::auto_ptr<SprueEngine::FilterableBlockMap<SprueEngine::RGBA>> blockmap(new SprueEngine::FilterableBlockMap<SprueEngine::RGBA>(width, height));

        for (int i = 0; i < meshData->Length; ++i)
        {
            auto mesh = meshData[i];
            auto actualMeshData = mesh->GetInternalMeshData();

            SprueEngine::CurvatureBaker baker(actualMeshData->BuildHalfEdgeMesh(), actualMeshData);
            baker.SetWidth(width);
            baker.SetHeight(height);
            baker.Bake(blockmap.get(), cancelFunc);
        }

        auto ret = gcnew ImageData();
        ret->Resize(width, height);
        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                auto pixel = blockmap->getData()[y * width + x];
                ret->Pixels[y * width + x] = Microsoft::Xna::Framework::Color(pixel.r, pixel.g, pixel.b, pixel.a);
            }
        }
        return ret;
    }

    ImageData^ Bakers::DominantPlane(array<MeshData^>^ meshData, int width, int height, BakingProgressCallback^ callback)
    {
        return nullptr;
    }

    ImageData^ Bakers::Facet(array<MeshData^>^ meshData, int width, int height, float angleTolerance, bool forceAllEdges, BakingProgressCallback^ callback)
    {
        SprueEngine::BAKER_CANCELATION cancelFunc = callback != nullptr ? static_cast<SprueEngine::BAKER_CANCELATION>(Marshal::GetFunctionPointerForDelegate(callback).ToPointer()) : 0x0;

        std::auto_ptr<SprueEngine::FilterableBlockMap<SprueEngine::RGBA>> blockmap(new SprueEngine::FilterableBlockMap<SprueEngine::RGBA>(width, height));
        blockmap->fill(SprueEngine::RGBA(0, 0, 0, 0));

        for (int i = 0; i < meshData->Length; ++i)
        {
            auto mesh = meshData[i];
            auto actualMeshData = mesh->GetInternalMeshData();

            SprueEngine::FacetBaker baker(actualMeshData);
            baker.SetAllEdges(forceAllEdges);
            baker.SetAngleThreshold(angleTolerance);
            baker.SetWidth(width);
            baker.SetHeight(height);
            baker.Bake(blockmap.get(), cancelFunc);
        }

        auto ret = gcnew ImageData();
        ret->Resize(width, height);
        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                auto pixel = blockmap->getData()[y * width + x];
                ret->Pixels[y * width + x] = Microsoft::Xna::Framework::Color(pixel.r, pixel.g, pixel.b, pixel.a);
            }
        }
        return ret;
    }

    ImageData^ Bakers::ObjectSpaceGradient(array<MeshData^>^ meshData, int width, int height, BakingProgressCallback^ callback)
    {
        SprueEngine::BAKER_CANCELATION cancelFunc = callback != nullptr ? static_cast<SprueEngine::BAKER_CANCELATION>(Marshal::GetFunctionPointerForDelegate(callback).ToPointer()) : 0x0;

        std::auto_ptr<SprueEngine::FilterableBlockMap<SprueEngine::RGBA>> blockmap(new SprueEngine::FilterableBlockMap<SprueEngine::RGBA>(width, height));

        for (int i = 0; i < meshData->Length; ++i)
        {
            auto mesh = meshData[i];
            auto actualMeshData = mesh->GetInternalMeshData();

            SprueEngine::ObjectSpaceGradientBaker baker(actualMeshData->BuildHalfEdgeMesh(), actualMeshData);
            baker.SetWidth(width);
            baker.SetHeight(height);
            baker.Bake(blockmap.get(), cancelFunc);
        }

        auto ret = gcnew ImageData();
        ret->Resize(width, height);
        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                auto pixel = blockmap->getData()[y * width + x];
                ret->Pixels[y * width + x] = Microsoft::Xna::Framework::Color(pixel.r, pixel.g, pixel.b, pixel.a);
            }
        }
        return ret;
    }

    ImageData^ Bakers::ObjectSpaceNormal(array<MeshData^>^ meshData, int width, int height, BakingProgressCallback^ callback)
    {
        SprueEngine::BAKER_CANCELATION cancelFunc = callback != nullptr ? static_cast<SprueEngine::BAKER_CANCELATION>(Marshal::GetFunctionPointerForDelegate(callback).ToPointer()) : 0x0;

        std::auto_ptr<SprueEngine::FilterableBlockMap<SprueEngine::RGBA>> blockmap(new SprueEngine::FilterableBlockMap<SprueEngine::RGBA>(width, height));

        for (int i = 0; i < meshData->Length; ++i)
        {
            auto mesh = meshData[i];
            auto actualMeshData = mesh->GetInternalMeshData();

            SprueEngine::ObjectSpaceNormalBaker baker(actualMeshData->BuildHalfEdgeMesh(), actualMeshData);
            baker.SetWidth(width);
            baker.SetHeight(height);
            baker.Bake(blockmap.get(), cancelFunc);
        }

        auto ret = gcnew ImageData();
        ret->Resize(width, height);
        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                auto pixel = blockmap->getData()[y * width + x];
                ret->Pixels[y * width + x] = Microsoft::Xna::Framework::Color(pixel.r, pixel.g, pixel.b, pixel.a);
            }
        }
        return ret;
    }
    ImageData^ Bakers::ObjectSpacePosition(array<MeshData^>^ meshData, int width, int height, BakingProgressCallback^ callback)
    {
        SprueEngine::BAKER_CANCELATION cancelFunc = callback != nullptr ? static_cast<SprueEngine::BAKER_CANCELATION>(Marshal::GetFunctionPointerForDelegate(callback).ToPointer()) : 0x0;

        std::auto_ptr<SprueEngine::FilterableBlockMap<SprueEngine::RGBA>> blockmap(new SprueEngine::FilterableBlockMap<SprueEngine::RGBA>(width, height));

        for (int i = 0; i < meshData->Length; ++i)
        {
            auto mesh = meshData[i];
            auto actualMeshData = mesh->GetInternalMeshData();

            SprueEngine::ObjectSpacePositionBaker baker(actualMeshData->BuildHalfEdgeMesh(), actualMeshData);
            baker.SetWidth(width);
            baker.SetHeight(height);
            baker.Bake(blockmap.get(), cancelFunc);
        }

        auto ret = gcnew ImageData();
        ret->Resize(width, height);
        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                auto pixel = blockmap->getData()[y * width + x];
                ret->Pixels[y * width + x] = Microsoft::Xna::Framework::Color(pixel.r, pixel.g, pixel.b, pixel.a);
            }
        }
        return ret;
    }

}