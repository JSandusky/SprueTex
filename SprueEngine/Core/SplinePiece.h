#pragma once

#include <SprueEngine/ClassDef.h>
#include <SprueEngine/Core/SpruePieces.h>

namespace SprueEngine
{

    /// Uses a Catmull-Rom spline to definie a spinal/caudal type shape.
    SHARP_BIND class SPRUE SplinePiece : public SHARP_BASE SkeletalPiece
    {
        BASECLASSDEF(SplinePiece, SkeletalPiece);
        NOCOPYDEF(SplinePiece);
        SPRUE_EDITABLE(SplinePiece);
    public:
        /// Construct.
        SplinePiece();
        /// Destruct.
        virtual ~SplinePiece();

        /// Register the factory and properties.
        static void Register(Context* context);

        /// Gets the amount of desired spacing interval for skeleton bone creation along the spine.
        float GetBoneSpacing() const { return boneSpacing_; }
        /// Sets the amount of desired spacing interval for skeleton bone creation along the spine.
        void SetBoneSpacing(float value) { boneSpacing_ = value; }

        /// Returns true if generated bones will be flagged as being spine bodies.
        bool IsMarkAsSpine() const { return markAsSpine_; }
        /// Sets whether generated bones should be specified as spine bodies.
        void SetMarkAsSpine(bool value) { markAsSpine_ = value; }

        virtual BoundingBox ComputeBounds() const override { return bounds_; }

        float GetDensity(const Vec3& pos) const override { return 1000.0f; }

        float GetLength() const { return 10.0f; }
        Vec3 GetValue(float time) const { return Vec3(); }
        unsigned GetNearestSegmentIndex(const Vec3& pos, float segmentDelta) const;

    private:
        /// Spacing interval for generated bones.
        float boneSpacing_ = 0.2f;
        /// If set to true then when bones are generated they will be marked as spine bodies.
        bool markAsSpine_ = true;
    };

}