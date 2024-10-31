#pragma once

#include <SprueEngine/Core/SceneObject.h>
#include <SprueEngine/MathGeoLib/AllMath.h>

namespace SprueEngine
{

class Context;

/// Uses a spline with radius to deform sampling points within the spline
class WireDeformer : public SceneObject
{
    BASECLASSDEF(WireDeformer, SceneObject);
    NOCOPYDEF(WireDeformer);
    SPRUE_EDITABLE(WireDeformer);
public:
    WireDeformer();
    virtual ~WireDeformer();

    static void Register(Context*);

    virtual void Deform(Vec3&) const override;

    std::vector<Vec3>& GetControlPoints() { return controlPoints_; }
    const std::vector<Vec3>& GetControlPoints() const { return controlPoints_; }

    std::vector<Vec3>& GetWarpedPoints() { return warpedControlPoints_; }
    const std::vector<Vec3>& GetWarpedPoints() const { return warpedControlPoints_; }

    virtual float GetRadius() const { return radius_; }

    virtual void SetRadius(float value) { radius_ = value; }

    virtual bool AcceptsChildren() const { return false; }

    virtual BoundingBox ComputeBounds() const override;

    unsigned GetPointCount() const { return controlPoints_.size(); }
    void SetPointCount(unsigned ct);

protected:
    virtual void GetGizmos(std::vector<std::shared_ptr<Gizmo> >& gizmos) override;

private:
    struct WireVertexGizmo;

    /// Get the value of the bezier curve at the given point in 0.0-1.0 normalized time
    Vec3 SolveBezier(const std::vector<Vec3>& points, float time) const;
    /// Use gradient descent to find the closet reasonable point on the spline
    float ClosestTime(const Vec3& inputPoint, float& bestDistance) const;
    
    float radius_;
    std::vector<Vec3> controlPoints_;
    std::vector<Vec3> warpedControlPoints_;
};

}