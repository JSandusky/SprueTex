#include "GradientColorProjector.h"

#include "../Context.h"
#include "../../IDebugRender.h"

namespace SprueEngine
{

    GradientColorProjector::GradientColorProjector()
    {
        // Initialize colors
        colors_[0] = colors_[1] = RGBA(1, 1, 1);
    }

    GradientColorProjector::~GradientColorProjector()
    {

    }

    void GradientColorProjector::Register(Context* context)
    {
        context->RegisterFactory<GradientColorProjector>("GradientColorProjector", "");
        COPY_PROPERTIES(TexturingComponent, GradientColorProjector);
        REGISTER_PROPERTY_MEMORY(GradientColorProjector, RGBA, offsetof(GradientColorProjector, colors_[0]), RGBA(1, 1, 1), "Upper Color", "", PS_VisualConsequence | PS_TextureConsequence);
        REGISTER_PROPERTY_MEMORY(GradientColorProjector, RGBA, offsetof(GradientColorProjector, colors_[1]), RGBA(1, 1, 1), "Lower Color", "", PS_VisualConsequence | PS_TextureConsequence);
        REGISTER_PROPERTY_MEMORY(GradientColorProjector, bool, offsetof(GradientColorProjector, normalMode_), true, "Base on Normals", "Color transition will be based on vertex normals instead of position between the planes", PS_VisualConsequence | PS_TextureConsequence);

        context->RemoveProperty("GradientColorProjector", "Material");
        context->RemoveProperty("GradientColorProjector", "Image");
        context->RemoveProperty("GradientColorProjector", "Mask");
    }

    BoundingBox GradientColorProjector::ComputeBounds() const
    {
        BoundingBox bb(Vec3(-1, -1, -1), Vec3(1, 1, 1));
        bounds_ = bb;
        return bounds_;
    }

    void GradientColorProjector::DrawDebug(IDebugRender* renderer, unsigned flags) const
    {
        RGBA color = flags & SPRUE_DEBUG_HOVER ? RGBA::Gold : RGBA::Green;
        color.a = 0.75f;
        if (flags & SPRUE_DEBUG_PASSIVE)
            color.a = 0.1f;

        renderer->DrawBoundingBox(GetWorldTransform(), BoundingBox(Vec3(-1, -1, -1), Vec3(1, 1, 1)), color);

        if (flags == 0)
        {
            auto rotateMat = GetWorldTransform().RotatePart();
            const float radius = GetWorldTransform().GetScale().MaxElement() * 0.25f;
            renderer->DrawDisc(GetWorldTransform() * Vec3(0, 1, 0), radius,  rotateMat * Vec3(0, 1, 0), colors_[0]);
            renderer->DrawDisc(GetWorldTransform() * Vec3(0, -1, 0), radius, rotateMat * Vec3(0, -1, 0), colors_[1]);
        }
    }

    RGBA GradientColorProjector::SampleColorProjection(const Vec3& position, const Vec3& normal) const
    {
        auto trans = GetWorldTransform().Inverted();
        const Vec3 localPos = trans * position;
        const Vec3 localNormal = (trans.RotatePart() * normal).Normalized();

        BoundingBox bounds(Vec3(-1, -1, -1), Vec3(1, 1, 1));
        if (bounds.Contains(localPos))
        {
            if (normalMode_)
                return SprueLerp(colors_[1], colors_[0], NORMALIZE(localNormal.y, -1.0f, 1.0f));
            else
                return SprueLerp(colors_[1], colors_[0], NORMALIZE(localPos.y, -1.0f, 1.0f));
        }

        return RGBA::Invalid;
    }

}