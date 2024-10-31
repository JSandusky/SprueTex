#pragma once

#include <SprueEngine/Core/SpruePieces.h>

namespace SprueEngine
{

/// Spherical volume inside which billbards may be placed
/// Similar to early versions of speedtree
/// Is never meshed
class SPRUE BillboardCloudPiece : public SpruePiece
{
    BASECLASSDEF(BillboardCloudPiece, SpruePiece);
    NOCOPYDEF(BillboardCloudPiece);
    SPRUE_EDITABLE(BillboardCloudPiece);
public:
    BillboardCloudPiece();
    virtual ~BillboardCloudPiece();

    static void Register(Context* context);

    virtual BoundingBox ComputeBounds() const override { return bounds_ = BoundingBox(); }

    virtual bool IsMeshed() const override { return false; }

    unsigned GetBillboardCount() const { return billboardCount_; }
    void SetBillboardCount(unsigned count) { billboardCount_ = count; }

protected:
    virtual float GetDensity(const Vec3&) const override { return 0.0f; }

private:
    unsigned billboardCount_ = 10;
};

}