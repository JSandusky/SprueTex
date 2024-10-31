#include "WireDeformer.h"

#include <SprueEngine/Core/Context.h>
#include <SprueEngine/Math/MathDef.h>
#include <limits>

namespace SprueEngine
{

struct WireDeformer::WireVertexGizmo : public Gizmo
{
    unsigned index_;
    Vec3* target;
    WireVertexGizmo(WireDeformer* cage, Vec3* target, bool deform) : Gizmo(cage, GIZ_Translate | (deform ? GIZ_DeformedControlPoint : GIZ_InitialControlPoint)), target(target)
    {
        transform_ = Mat3x4::FromTRS(cage->GetWorldTransform() * (*target), Quat(), Vec3::one);
    }

    virtual bool UpdateValue() override
    {
        WireDeformer* wire = (WireDeformer*)source_;
        Mat3x4 inverseMat = wire->GetWorldTransform().Inverted();

        *target = (inverseMat * transform_.TranslatePart()).xyz();

        return true;
    }

    virtual void RefreshValue() override
    {
        WireDeformer* wire = (WireDeformer*)source_;
        transform_ = Mat3x4::FromTRS(wire->GetWorldTransform() * (*target), Quat(), Vec3::one);
    }

    virtual bool Equals(Gizmo* rhs) const override
    {
        if (auto other = dynamic_cast<WireVertexGizmo*>(rhs))
            return source_ == rhs->source_ && index_ == other->index_;        
        return false;
    }
};

void WireDeformer::Register(Context* context)
{
    context->RegisterFactory<WireDeformer>("WireDeformer", "Deforms space around a spline wire");
    context->CopyBaseProperties(StringHash("SceneObject"), StringHash("WireDeformer"));
    REGISTER_PROPERTY(WireDeformer, unsigned, GetPointCount, SetPointCount, 2, "Points", "", PS_VisualConsequence);
}

WireDeformer::WireDeformer()
{
    controlPoints_.push_back(Vec3(0, 1, 0));
    warpedControlPoints_.push_back(Vec3(0, 1, 0));

    controlPoints_.push_back(Vec3(0, 2, 0));
    warpedControlPoints_.push_back(Vec3(0, 2, 0));
}

WireDeformer::~WireDeformer()
{

}

void WireDeformer::Deform(Vec3& pos) const
{
    Vec3 pt(pos);
    pt = GetWorldTransform().Inverted() * pt;
    
    float startDist = 0.0f; // Warp strength may be rated by distance
    float startTime = ClosestTime(pt, startDist);
    
    Vec3 startPoint = SolveBezier(controlPoints_, startTime);
    Vec3 endPoint = SolveBezier(warpedControlPoints_, startTime);

    pt = pt + endPoint - startPoint;
    pt = GetWorldTransform() * pt;

    pos.x = pt.x;
    pos.y = pt.y;
    pos.z = pt.z;
}

Vec3 WireDeformer::SolveBezier(const std::vector<Vec3>& points, float time) const
{
    if (points.size() == 2)
        return SprueLerp(points[0], points[1], time);
    else
    {
        std::vector<Vec3> interpolatedKnots;
        for (unsigned i = 1; i < points.size(); i++)
            interpolatedKnots.push_back(SprueLerp(points[i - 1], points[i], time));
        return SolveBezier(interpolatedKnots, time);
    }
}

#define GD_TRIES 30
static const float Spline_StepSize = 1.0f / 30.0f;

float WireDeformer::ClosestTime(const Vec3& inputPoint, float& bestDistance) const
{
    float currentSample = 0.0f;
    bestDistance = (inputPoint - SolveBezier(controlPoints_, currentSample)).LengthSq();
    for (unsigned i = 0; i < GD_TRIES; ++i)
    {
        float step = CLAMP01(currentSample + Spline_StepSize);
        float newDistance = (inputPoint - SolveBezier(controlPoints_, step)).LengthSq();
        if (newDistance < bestDistance)
        {
            currentSample = step;
            bestDistance = newDistance;
        }
    }
    return currentSample;
}

BoundingBox WireDeformer::ComputeBounds() const
{
    BoundingBox bounds;

    for (auto vert : controlPoints_)
    {
        bounds.Enclose(vert - radius_);
        bounds.Enclose(vert + radius_);
    }
    for (auto vert : warpedControlPoints_)
    {
        bounds.Enclose(vert - radius_);
        bounds.Enclose(vert + radius_);
    }

    return bounds_ = bounds;
}

void WireDeformer::GetGizmos(std::vector<std::shared_ptr<Gizmo> >& gizmos)
{
    base::GetGizmos(gizmos);
    for (unsigned i = 0; i < controlPoints_.size(); ++i)
        gizmos.push_back(std::shared_ptr<Gizmo>(new WireVertexGizmo(this, &controlPoints_[i], false)));
    for (unsigned i = 0; i < warpedControlPoints_.size(); ++i)
        gizmos.push_back(std::shared_ptr<Gizmo>(new WireVertexGizmo(this, &warpedControlPoints_[i], true)));
}

void WireDeformer::SetPointCount(unsigned ct)
{
    controlPoints_.resize(ct);
    warpedControlPoints_.resize(ct);
}

}