#include "RingProjector.h"

#include <SprueEngine/Core/Context.h>
#include <SprueEngine/IDebugRender.h>

#include <math.h>

#define POINT_ON_SPHERE(sphere, theta, phi) Vec3( \
        sphere.pos.x + sphere.r * sinf((float)(theta)  * DEG_TO_RAD) * sinf((float)(phi) * DEG_TO_RAD), \
        sphere.pos.y + sphere.r * cosf((float)(phi) * DEG_TO_RAD), \
        sphere.pos.z + sphere.r * cosf((float)(theta) * DEG_TO_RAD) * sinf((float)(phi) * DEG_TO_RAD))

namespace SprueEngine
{

    RingProjector::RingProjector() :
        TexturingComponent()
    {

    }

    RingProjector::~RingProjector()
    {

    }

    void RingProjector::Register(Context* context)
    {
        context->RegisterFactory<RingProjector>("RingProjector", "");
        context->CopyBaseProperties("TexturingComponent", "RingProjector");
    }

    BoundingBox RingProjector::ComputeBounds() const
    {
        BoundingBox bb(Vec3(-1, -1, -1), Vec3(1, 1, 1));
        bb.TransformAsAABB(GetWorldTransform());
        return bb;
    }

    void RingProjector::DrawDebug(IDebugRender* renderer, unsigned flags) const
    {
        RGBA color = flags & SPRUE_DEBUG_HOVER ? RGBA::Gold : RGBA::Green;
        color.a = 0.75f;
        if (flags & SPRUE_DEBUG_PASSIVE)
            color.a = 0.1f;

        auto transform = GetWorldTransform();

        Sphere sphere;
        sphere.r = 1.0f;
        Vec3 heightVec(0, 1.0f, 0);
        Vec3 offsetXVec(1.0f, 0, 0);
        Vec3 offsetZVec(0, 0, 1.0f);
        for (float i = 0; i < 360; i += 22.5f)
        {
            Vec3 p1 = POINT_ON_SPHERE(sphere, i, 90);
            Vec3 p2 = POINT_ON_SPHERE(sphere, i + 22.5f, 90);
            renderer->DrawLine(transform * (p1 - heightVec), transform * (p2 - heightVec), color);
            renderer->DrawLine(transform * (p1 + heightVec), transform * (p2 + heightVec), color);
        }
        renderer->DrawLine(transform * (-heightVec + offsetXVec), transform * (heightVec + offsetXVec), color);
        renderer->DrawLine(transform * (-heightVec + -offsetXVec), transform * (heightVec - offsetXVec), color);
        renderer->DrawLine(transform * (-heightVec + offsetZVec), transform * (heightVec + offsetZVec), color);
        renderer->DrawLine(transform * (-heightVec + -offsetZVec), transform * (heightVec - offsetZVec), color);
    }

    RGBA RingProjector::SampleColorProjection(const Vec3& position, const Vec3& normal) const
    {
        if (!GetImageData() || !GetImageData()->GetImage())
            return RGBA::Invalid;

        const auto trans = GetWorldTransform().Inverted();
        const Vec3 localPosition = trans * position;
        const Vec3 localNormal = (trans.RotatePart() * normal).Normalized();

        if (localPosition.y < -1.0f || localPosition.y > 1.0f)
            return RGBA::Invalid;

        float u = atan2(localPosition.x, localPosition.z) / (2 * PI) + 0.5f;
        float v = (localPosition.y * 0.5f) + 0.5f;
        return GetImageData()->GetImage()->getBilinear(u, v);
    }
}