#pragma once

#include <SprueEngine/Math/IntersectionInfo.h>
#include <SprueEngine/Core/SpruePieces.h>

namespace SprueEngine
{
    /// An instance piece references another piece (potentially just a "library piece" and evaluates
    /// that piece first before evaluating any children against it
    class SPRUE InstancePiece : public SHARP_BASE SpruePiece
    {
        BASECLASSDEF(InstancePiece, SpruePiece);
        NOCOPYDEF(InstancePiece);
        SPRUE_EDITABLE(InstancePiece);
    public:
        InstancePiece(SpruePiece* refPiece);
        InstancePiece();
        virtual ~InstancePiece();

        static void Register(Context*);

        SpruePiece* GetReferencePiece() { return referencePiece_; }
        const SpruePiece* GetReferencePiece() const { return referencePiece_; }
        void SetReferencePiece(SpruePiece* newPiece);

        virtual BoundingBox ComputeBounds() const override;

        virtual bool TestRayAccurate(const Ray& ray, IntersectionInfo* info) const override;

        virtual void DrawDebug(IDebugRender* renderer, unsigned flags = 0) const override;

    protected:
        virtual float GetDensity(const Vec3&) const override;
        virtual unsigned CalculateStructuralHash() const override { return (unsigned)referencePiece_ * 31; }

    private:
        SpruePiece* referencePiece_;
    };

}