#pragma once

#include <SprueEngine/ClassDef.h>
#include <SprueEngine/IEditable.h>
#include <SprueEngine/IMeshable.h>
#include <SprueEngine/Math/MathDef.h>
#include <SprueEngine/MathGeoLib/AllMath.h>

#include <map>
#include <string>

namespace SprueEngine
{
    struct SceneObjectDebugLine;
    class Context;
    class Deserializer;
    class IDebugRender;
    class Serializer;

    struct ShapeParams
    {
        float alpha;
        float r;
        float x;
        float y;
        float z;
    };

    enum DensityParamFlags
    {
        DPF_None = 0,
        DPF_XAsVec3 = 1 << 0,
        DPF_XAsVec2 = 1 << 1,
        DPF_AsVec4 = 1 << 2
    };

    typedef float(*DENSITY_FUNCTION)(const Vec3& pos, const ShapeParams& params);

    /// List of simple density functions for persisting density function
    enum DensityFunction
    {
        DF_Sphere,
        DF_Cube,
        DF_RoundedBox,
        DF_Capsule,
        DF_Cylinder,
        DF_Cone,
        DF_CappedCone,
        DF_Plane,
        DF_Ellipsoid,
        DF_Torus,
        DF_SuperEllipsoid
    };

    float SphereDensity(const Vec3& pos, const ShapeParams& params);
    float CubeDensity(const Vec3& pos, const ShapeParams& params);
    float RoundedBoxDensity(const Vec3& pos, const ShapeParams& params);
    float CapsuleDensity(const Vec3& pos, const ShapeParams& params);
    float CylinderDensity(const Vec3& pos, const ShapeParams& params);
    float ConeDensity(const Vec3& pos, const ShapeParams& params);
    float CappedConeDensity(const Vec3& p, const ShapeParams& params);
    float PlaneDistance(const Vec3& p, const ShapeParams& params);
    float EllipsoidDistance(const Vec3& p, const ShapeParams& params);
    float TorusDensity(const Vec3& pos, const ShapeParams& params);
    float SuperEllipseDensity(const Vec3& pos, const ShapeParams& params);

    Vec3 Twist(const Vec3& p, float amount);
    Vec3 CheapBend(const Vec3& p, float amount);

    float Displace(float lhsDistance, float rhsDistance, float displacementPower);
    float Blend(float lhsDistance, float rhsDistance, float blendPower);

    /// Wrapper class surrounding a density function, because all parameters take the same values this works
    class SPRUE DensityHandler : public IEditable, public IMeshable
    {
    protected:
        DensityHandler(DENSITY_FUNCTION function) : function(function) { }
    public:
        ShapeParams params;
        DENSITY_FUNCTION function;

        virtual float CalculateDensity(const Vec3& pos) const override {
            if (function)
                return function(pos, params);
            return FLT_MAX;
        }

        virtual BoundingBox CalculateBounds() const
        {
            BoundingBox ret(
                Vec3(
                    -(1000.0f - CalculateDensity(Vec3::NegativeX * 1000.0f)),
                    -(1000.0f - CalculateDensity(Vec3::NegativeY * 1000.0f)),
                    -(1000.0f - CalculateDensity(Vec3::NegativeZ * 1000.0f))),
                Vec3(
                    1000.0f - CalculateDensity(Vec3::PositiveX * 1000.0f),
                    1000.0f - CalculateDensity(Vec3::PositiveY * 1000.0f),
                    1000.0f - CalculateDensity(Vec3::PositiveZ * 1000.0f)));
            return ret;
        }

        virtual std::shared_ptr<MeshData> GeneratePreviewMesh() = 0;
        virtual std::string ToString() const = 0;
        virtual void WriteParameters(Serializer* serializer) = 0;
        virtual void DrawDebug(IDebugRender* renderer, const Mat3x4& transform, const RGBA& color);
    };

#define DECL_DENSITY(NAME, FUNC) class SPRUE NAME : public DensityHandler { public: virtual StringHash GetTypeHash() const { return StringHash( #NAME ); } virtual const char* GetTypeName() const { return #NAME; } public: static void Register(Context*); NAME() : DensityHandler(FUNC)
#define END_DENSITY };
    DECL_DENSITY(SphereFunction, SphereDensity)
    {
        params.r = 1.0f;
    }
        float getRadius() const { return params.r; }
        void setRadius(float value) { params.r = value; }
        virtual std::shared_ptr<MeshData> GeneratePreviewMesh() override;
        virtual std::string ToString() const override;
        virtual void WriteParameters(Serializer* serialzier) override;
        virtual void DrawDebug(IDebugRender* renderer, const Mat3x4& transform, const RGBA& color) override;
    END_DENSITY

    DECL_DENSITY(CubeFunction, CubeDensity)
    {
        params.x = 1.0f;
        params.y = 1.0f;
        params.z = 1.0f;
    }
        Vec3 getSize() const { return *(Vec3*)&params.x; }
        void setSize(Vec3 size) { memcpy(&params.x, &size, sizeof(Vec3)); }
        virtual std::shared_ptr<MeshData> GeneratePreviewMesh() override;
        virtual std::string ToString() const override;
        virtual void WriteParameters(Serializer* serialzier) override;
        virtual void DrawDebug(IDebugRender* renderer, const Mat3x4& transform, const RGBA& color) override;
    END_DENSITY

    DECL_DENSITY(RoundedBoxFunction, RoundedBoxDensity)
    {
        params.x = 1.0f;
        params.y = 1.0f;
        params.z = 1.0f;
        params.r = 0.2f;
    }
        Vec3 getSize() const { return *(Vec3*)&params.x; }
        void setSize(Vec3 size) { memcpy(&params.x, &size, sizeof(Vec3)); }
        float getRadius() const { return params.r; }
        void setRadius(float value) { params.r = value; }
        virtual std::shared_ptr<MeshData> GeneratePreviewMesh() override;
        virtual std::string ToString() const override;
        virtual void WriteParameters(Serializer* serialzier) override;
        virtual void DrawDebug(IDebugRender* renderer, const Mat3x4& transform, const RGBA& color) override;
    END_DENSITY

    DECL_DENSITY(CapsuleFunction, CapsuleDensity)
    {
        params.r = 0.5f;
        params.x = 1.0f;
        params.y = 1.0f;
        params.z = 1.0f;
    }
        Vec3 getSize() const { return *(Vec3*)&params.x; }
        void setSize(Vec3 size) { memcpy(&params.x, &size, sizeof(Vec3)); }
        float getRadius() const { return params.r; }
        void setRadius(float value) { params.r = value; }
        virtual std::shared_ptr<MeshData> GeneratePreviewMesh() override;
        virtual std::string ToString() const override;
        virtual void WriteParameters(Serializer* serialzier) override;
        virtual void DrawDebug(IDebugRender* renderer, const Mat3x4& transform, const RGBA& color) override;
    END_DENSITY

    DECL_DENSITY(CylinderFunction, CylinderDensity)
    {
        params.x = 1.0f;
        params.y = 1.0f;
        params.z = 1.0f;
        params.r = 1.0f;
    }
        Vec3 getSize() const { return *(Vec3*)&params.x; }
        void setSize(Vec3 size) { memcpy(&params.x, &size, sizeof(Vec3)); }
        virtual std::shared_ptr<MeshData> GeneratePreviewMesh() override;
        virtual std::string ToString() const override;
        virtual void WriteParameters(Serializer* serialzier) override;
        virtual void DrawDebug(IDebugRender* renderer, const Mat3x4& transform, const RGBA& color) override;
    END_DENSITY

    DECL_DENSITY(ConeFunction, ConeDensity)
    {
        //params.r = 1.0f;
        params.x = 0.8f;
        params.y = 0.6f;
        params.z = 0.3f;
        params.r = 1.0f;
    }
        float getRadius() const { return params.r; }
        void setRadius(float value) { params.r = value; }
        virtual std::shared_ptr<MeshData> GeneratePreviewMesh() override;
        virtual std::string ToString() const override;
        virtual void WriteParameters(Serializer* serialzier) override;
        virtual void DrawDebug(IDebugRender* renderer, const Mat3x4& transform, const RGBA& color) override;
    END_DENSITY

    DECL_DENSITY(CappedConeFunction, CappedConeDensity)
    {
        params.r = 1.0f;
    }
        float getRadius() const { return params.r; }
        void setRadius(float value) { params.r = value; }
        virtual std::shared_ptr<MeshData> GeneratePreviewMesh() override;
        virtual std::string ToString() const override;
        virtual void WriteParameters(Serializer* serialzier) override;
    END_DENSITY

    DECL_DENSITY(PlaneFunction, PlaneDistance)
    {

    }
        Plane getPlane() const { return *(Plane*)&params.r; }
        void setPlane(Plane plane) { memcpy(&params.r, &plane, sizeof(Plane)); }
        virtual std::shared_ptr<MeshData> GeneratePreviewMesh() override;
        virtual std::string ToString() const override;
        virtual void WriteParameters(Serializer* serialzier) override;
        virtual void DrawDebug(IDebugRender* renderer, const Mat3x4& transform, const RGBA& color) override;
    END_DENSITY

    DECL_DENSITY(EllipsoidFunction, EllipsoidDistance)
    {
        params.x = 1.0f;
        params.y = 1.0f;
        params.z = 1.0f;
        params.r = 0.0f;
    }
        Vec3 getSize() const { return *(Vec3*)&params.x; }
        void setSize(Vec3 size) { memcpy(&params.x, &size, sizeof(Vec3)); }
        float getRounding() const { return params.r; }
        void setRounding(float value) { params.r = value; }
        virtual std::shared_ptr<MeshData> GeneratePreviewMesh() override;
        virtual std::string ToString() const override;
        virtual void WriteParameters(Serializer* serialzier) override;
        virtual void DrawDebug(IDebugRender* renderer, const Mat3x4& transform, const RGBA& color) override;
    END_DENSITY

    DECL_DENSITY(TorusFunction, TorusDensity)
    {
        params.x = 1.0f;
        params.y = 0.5f;
    }
        float getInnerRadius() const { return params.x; }
        void setInnerRadius(float value) { params.x = value; }
        float getOuterRadius() const { return params.y; }
        void setOuterRadius(float value) { params.y = value; }
        virtual std::shared_ptr<MeshData> GeneratePreviewMesh() override;
        virtual std::string ToString() const override;
        virtual void WriteParameters(Serializer* serialzier) override;
        virtual void DrawDebug(IDebugRender* renderer, const Mat3x4& transform, const RGBA& color) override;
    END_DENSITY

    DECL_DENSITY(SuperEllipseFunction, SuperEllipseDensity)
    {
        params.x = 1.0f;
        params.y = 1.0f;
        params.z = 1.0f;
        params.r = 1.0f;
        params.alpha = 0.25f;
    }
        float getN() const { return params.alpha; }
        void setN(float value) { params.alpha = value; }
        float getExponent() const { return params.r; }
        void setExponent(float value) { params.r = value;}
        Vec3 getSize() const { return *(Vec3*)&params.x; }
        void setSize(Vec3 size) { memcpy(&params.x, &size, sizeof(Vec3)); }
        virtual std::shared_ptr<MeshData> GeneratePreviewMesh() override;
        virtual std::string ToString() const override;
        virtual void WriteParameters(Serializer* serialzier) override;
    END_DENSITY

#undef DECL_DENSITY
#undef END_DENSITY

    void RegisterDensityFunctions(Context*);
}