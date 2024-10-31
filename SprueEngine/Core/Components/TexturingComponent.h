#pragma once

#include <SprueEngine/Core/Component.h>
#include <SprueEngine/Geometry/Material.h>
#include <SprueEngine/Loaders/BasicImageLoader.h>

namespace SprueEngine
{
    /// Baseclass for components that are used in texture generation.
    class SPRUE TexturingComponent : public Component
    {
        BASECLASSDEF(TexturingComponent, Component);
        NOCOPYDEF(TexturingComponent);
        SPRUE_EDITABLE(TexturingComponent);
    public:
        /// Construct.
        TexturingComponent();
        /// Destruct.
        virtual ~TexturingComponent();

        /// Register properties.
        static void Register(Context* context);

        ResourceHandle GetBitmapResourceHandle() const { return bitmapResourceHandle_; }
        void SetBitmapResourceHandle(const ResourceHandle& handle) { bitmapResourceHandle_ = handle; }
        std::shared_ptr<BitmapResource> GetImageData() const { return imageData_; }
        void SetImageData(const std::shared_ptr<BitmapResource>& img) { imageData_ = img; }

        ResourceHandle GetMaskResourceHandle() const { return maskResourceHandle_; }
        void SetMaskResourceHandle(const ResourceHandle& handle) { maskResourceHandle_ = handle; }
        std::shared_ptr<BitmapResource> GetMaskData() const { return maskData_; }
        void SetMaskData(const std::shared_ptr<BitmapResource>& img) { maskData_ = img; }

        virtual RGBA SampleColorProjection(const Vec3& position, const Vec3& normal) const = 0;

        Material* GetMaterial() { return &material_; }

        Variant GetMaterialProperty() const;
        void SetMaterialProperty(Variant var);

    protected:
        Vec2 ProjectOntoTriangleUV(const Vec3& localPosition, const Vec3& localNormal, const Vec3* vertices, const Vec2* uvs, float normalTolerance) const;

    private:
        Material material_;
        std::shared_ptr<BitmapResource> imageData_;
        std::shared_ptr<BitmapResource> maskData_;
        ResourceHandle bitmapResourceHandle_;
        ResourceHandle maskResourceHandle_;
    };

}