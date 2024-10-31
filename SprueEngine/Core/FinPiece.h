#pragma once

#include <SprueEngine/Core/SpruePieces.h>
#include <SprueEngine/Core/PolyObject.h>

namespace SprueEngine
{

    class MeshData;

/// A quad (or quad-strip) that may be placed following contour perpendicularly or in parallel
/// Used for hair/fur shells, or literal fins. Is never meshed.
class SPRUE FinPiece : public SpruePiece, public PolyObject
{
    BASECLASSDEF(FinPiece, SpruePiece);
    NOCOPYDEF(FinPiece);
    SPRUE_EDITABLE(FinPiece);
public:
    FinPiece();
    virtual ~FinPiece();

    static void Register(Context*);

    virtual BoundingBox ComputeBounds() const override { return bounds_ = BoundingBox(); }

    virtual bool IsMeshed() const override { return false; }

    bool IsClippedToMesh() const { return isClipped_; }
    void SetClippedToMesh(bool value) { isClipped_ = value; }

    bool IsTwoSided() const { return isTwoSided_; }
    void SetTwoSided(bool value) { isTwoSided_ = value; }

    VariantVector GetPoints() const;
    void SetPoints(const VariantVector& pts);

    virtual bool IsCircular() const override { return false; }

    struct SPRUE Node
    {
        Vec3 position_ = Vec3(0,0,0);
        Quat rotation_ = Quat::identity;
        float width_ = 1.0f;
    };

    std::vector<Node*>& GetNodes() { return nodes_; }
    const std::vector<Node*>& GetNodes() const { return nodes_; }

    MeshData* CreateMesh() const;

    virtual void DrawDebug(IDebugRender* debug, unsigned flags = 0) const override;

protected:
    virtual float GetDensity(const Vec3&) const override { return 0.0f; }
    virtual void GetGizmos(std::vector<std::shared_ptr<Gizmo> >& gizmos) override;

private:
    class FinGizmo;

    bool isClipped_ = false;
    bool isTwoSided_ = true;
    std::vector<Node*> nodes_;
};

}