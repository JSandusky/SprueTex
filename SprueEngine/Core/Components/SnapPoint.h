#pragma once

#include <SprueEngine/Core/Component.h>

namespace SprueEngine
{

    /// A snap point is a point that can be used for enabling snapping rules.
    /// It is essentially a point-to-point constraint, and used to solve an alignment plane to stick an object onto other objects.
    /// The alignment is either by point, line, or 3-point plane.
    /// No object will accept more than 3 snap-points (the number required to form a plane) as enabling such will produce situations that cannot be solved
    /// reasonably for too many different morphologies.
    /// If a single snap-point is present then rotation can be made in "quick" mode along any axis.
    /// If two snap-points are present than rotation can be made around the axis of between the snap points.
    /// If three snap-points are present than no rotation is possible.
    class SPRUE SnapPoint : public Component
    {
        BASECLASSDEF(SnapPoint, Component);
        NOCOPYDEF(SnapPoint);
        SPRUE_EDITABLE(SnapPoint);
    public:
        SnapPoint();
        virtual ~SnapPoint();

        static void Register(Context* context);

        virtual void DrawDebug(IDebugRender* renderer, unsigned flags = 0) const override;

        virtual BoundingBox ComputeBounds() const override { return BoundingBox(Vec3(0,0,0), Vec3(0,0,0)); }
    };

}