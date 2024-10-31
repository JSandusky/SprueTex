#pragma once

#include <SprueEngine/ClassDef.h>
#include <SprueEngine/MathGeoLib/AllMath.h>
#include <SprueEngine/Core/DensityFunctions.h>
#include <SprueEngine/Core/SceneObject.h>
#include <SprueEngine/ResponseCurve.h>

#include <set>
#include <vector>

namespace SprueEngine
{
    class Context; 
    class SprueModel;
    class SpruePiece;

    /// SpaceWarps modify the final shape by coordinates
    premiere class SPRUE SpaceDeformer : public SceneObject
    {
        NOCOPYDEF(SpaceDeformer);
        SPRUE_EDITABLE(SpaceDeformer);
        friend class SpruePiece;
        friend class SprueModel;
    public:
        SpaceDeformer();
        virtual ~SpaceDeformer();

        static void Register(Context*);
    
        bool IsBoneWarping() const { return warpBones_; }
        void SetBoneWarping(bool state) { warpBones_ = state; }

        virtual void Deform(Vec3&) const override;

        SprueModel* GetModel() const { return model_; }
        SpruePiece* GetParent() const { return parent_; }

        Vec3 GetOffset() const { return offset_; }
        Vec3 GetScaleAdjust() const { return scale_; }
        Quat GetRotationOffset() const { return rotation_; }
        float GetTwist() const { return twist_; }
        float GetBend() const { return bend_; }
        BoundingBox GetBounds() const { return bounds_; }
        void SetBounds(BoundingBox value) { bounds_ = value; }

        void SetOffset(const Vec3& value) { offset_ = value; UpdateWarpTransform(); }
        void SetScaleAdjust(const Vec3& value) { scale_ = value; UpdateWarpTransform(); }
        void SetRotationOffset(const Quat& value) { rotation_ = value; UpdateWarpTransform(); }
        void SetTwist(float twist) { twist_ = twist; }
        void SetBend(float bend) { bend_ = bend; }

        virtual bool AcceptsChildren() const { return false; }

        virtual BoundingBox ComputeBounds() const { return bounds_; }

    protected:
        void UpdateWarpTransform();
        void SetParent(SpruePiece* parent) { parent_ = parent; }
        void SetModel(SprueModel* model) { model_ = model; }

        bool warpBones_;
        SprueModel* model_;
        SpruePiece* parent_;
        Vec3 offset_;
        Vec3 scale_;
        Quat rotation_;
        float twist_;
        float bend_;
        Mat3x4 warpTrans_;
    };

}