#pragma once

#include <SprueEngine/ClassDef.h>
#include <SprueEngine/Core/DensityFunctions.h>
#include <SprueEngine/IEditable.h>
#include <SprueEngine/Math/IntersectionInfo.h>
#include <SprueEngine/Core/SceneObject.h>
#include <SprueEngine/Resource.h>
#include <SprueEngine/MathGeoLib/AllMath.h>

#include <vector>

namespace SprueEngine
{
    class Context;
    class Deserializer;
    class Serializer;

    /// Controls CSG operations
    enum ShapeMode
    {
        SM_Additive = 0,     // CSG Union
        SM_Subtractive = 1,  // CSG Subtract
        SM_Intersection = 2, // CSG Intersection
        SM_Displace = 3,     // Will be applied as a displacement
        SM_Blend = 4,        // Will be additively smoothed
    };

    /** SpruePieces in aggregate create a SprueModel

        CSG Operations
            CSG operations are performed in sequence
            Tree structure of SpruePieces allows grouping control
    */
    premiere SHARP_BIND class SPRUE SpruePiece : public SceneObject
    {
        NOCOPYDEF(SpruePiece);
        SPRUE_EDITABLE(SpruePiece);
        friend class SprueModel;
    protected:
        SpruePiece();

    public:
        virtual ~SpruePiece();

        static void Register(Context*);

        virtual float AdjustDensity(float currentDensity, const Vec3&) const override;

        ShapeMode GetMode() const { return mode_; }
        void SetMode(ShapeMode md) { mode_ = md; }

        virtual bool IsMeshed() const { return true; }

        virtual float CalculateDensity(const Vec3& position) const override;

        SprueModel* GetModel() const { return model_; }

    protected:        
        void SetModel(SprueModel* model) { model_ = model; }

        virtual float GetDensity(const Vec3&) const = 0;

        SprueModel* model_;
        SpruePiece* parent_;

        ShapeMode mode_;
    };

    SHARP_BIND class SPRUE FolderPiece : public SHARP_BASE SpruePiece
    {
        BASECLASSDEF(FolderPiece, SpruePiece);
        NOCOPYDEF(FolderPiece);
        SPRUE_EDITABLE(FolderPiece);
    public:
        FolderPiece() { }
        virtual ~FolderPiece() { }

        static void Register(Context*);

        virtual bool CanMove() const override { return false; }
        virtual bool CanRotate() const override { return false; }
        virtual bool CanScale() const override { return false; }

        virtual bool IsMeshed() const override { return false; }

        virtual float AdjustDensity(float currentDensity, const Vec3&) const override { return currentDensity; }

        /// Folders never really contain points, but need to return true to move down to the children
        virtual bool ContainsPoint(const Vec3& rhs) const { return true; }

        virtual BoundingBox ComputeBounds() const override { return bounds_ = BoundingBox(); }

    protected:
        virtual float GetDensity(const Vec3&) const override { return 0.0f; }
    };

    /// Abstract class for a sprue piece that contains a skeleton
    SHARP_BIND class SPRUE SkeletalPiece : public SHARP_BASE SpruePiece
    {
        BASECLASSDEF(SkeletalPiece, SpruePiece);
        NOCOPYDEF(SkeletalPiece);
        SPRUE_EDITABLE(SkeletalPiece);
    public:
        SkeletalPiece();
        virtual ~SkeletalPiece();

        static void Register(Context*);
    };

    /// Simple piece that uses a density function for defining a shape
    SHARP_BIND class SPRUE SimplePiece : public SHARP_BASE SpruePiece
    {
        BASECLASSDEF(SimplePiece, SpruePiece);
        NOCOPYDEF(SimplePiece);
        SPRUE_EDITABLE(SimplePiece);
    public:
        SimplePiece(DensityHandler* densityFunction);
        SimplePiece();
        virtual ~SimplePiece();

        static void Register(Context*);

        /// Gets the density handler instance, this function is preferred when writing code directly (non-GUI).
        DensityHandler* GetDensityHandler() const { return densityHandler_; }

        /// Sets the density handler instance, deleting any existing one. This function is preferred when writing code directly (non-GUI).
        void SetDensityHandler(DensityHandler* handler) { 
            if (densityHandler_)
                delete densityHandler_;
            densityHandler_ = handler; 
        }

        /// For property registration. Gets the density handler.
        Variant GetDensityHandlerProperty() const { return Variant((void*)densityHandler_); }

        /// For property registration. Sets the density handler but DOES NOT delete any existing density handler.
        void SetDensityHandlerProperty(Variant var) { densityHandler_ = (DensityHandler*)var.getVoidPtr(); }

        virtual BoundingBox ComputeBounds() const override {
            if (densityHandler_)
                bounds_ = densityHandler_->CalculateBounds();
            else
                bounds_ = BoundingBox();
            return bounds_;
        }

        virtual bool TestRayAccurate(const Ray& ray, IntersectionInfo* info) const override;

        virtual void DrawDebug(IDebugRender* debug, unsigned flags = 0) const override;

    protected:
        virtual unsigned CalculateStructuralHash() const { return densityHandler_->GetTypeHash().value_ ^ (mode_ * 31); }

        virtual float GetDensity(const Vec3& pos) const override {
            if (densityHandler_ == 0x0)
                return 500.0f;
            //Vec3 newPos = inverseWorldTransform_ * pos;
            return densityHandler_->function(pos, densityHandler_->params);
        }

    private:
        DensityHandler* densityHandler_;
    };

    /// A marker piece has no real purpose, it serves as a point marked in space, mostly just for runtime generation through the API
    /// so that templates can be created and parts attached at those marked points
    SHARP_BIND class SPRUE MarkerPiece : public SpruePiece
    {
        BASECLASSDEF(MarkerPiece, SpruePiece);
        NOCOPYDEF(MarkerPiece);
        SPRUE_EDITABLE(MarkerPiece);
    public:
        MarkerPiece();
        virtual ~MarkerPiece();

        static void Register(Context*);

        virtual bool IsMeshed() const final override { return false; }

        virtual bool AcceptsChildren() const override { return false; }

        virtual BoundingBox ComputeBounds() const override { return bounds_ = BoundingBox(); }

        virtual void DrawDebug(IDebugRender* renderer, unsigned flags = 0) const override;

    protected:
        virtual float GetDensity(const Vec3&) const { return 1000.0f; }
    };

#ifndef CppSharp
    SpruePiece* DeserializeSpruePiece(Deserializer* src);
    void SerializeSpruePiece(SpruePiece* piece, Serializer* dest);
#endif

}