#include "ColorCubeProjector.h"

#include "../Context.h"
#include "../../IDebugRender.h"

namespace SprueEngine
{

    ColorCubeProjector::ColorCubeProjector()
    {
        // Initialize colors
        for (int i = 0; i < 6; ++i)
            colors_[i] = RGBA(1, 1, 1);
    }
        
    ColorCubeProjector::~ColorCubeProjector()
    {

    }

    void ColorCubeProjector::Register(Context* context)
    {
        context->RegisterFactory<ColorCubeProjector>("ColorCubeProjector", "");
        COPY_PROPERTIES(TexturingComponent, ColorCubeProjector);
        REGISTER_PROPERTY_MEMORY(ColorCubeProjector, RGBA, offsetof(ColorCubeProjector, colors_[0]), RGBA(1, 1, 1), "Pos X", "", PS_VisualConsequence | PS_TextureConsequence);
        REGISTER_PROPERTY_MEMORY(ColorCubeProjector, RGBA, offsetof(ColorCubeProjector, colors_[1]), RGBA(1, 1, 1), "Neg X", "", PS_VisualConsequence | PS_TextureConsequence);
        REGISTER_PROPERTY_MEMORY(ColorCubeProjector, RGBA, offsetof(ColorCubeProjector, colors_[2]), RGBA(1, 1, 1), "Pos Y", "", PS_VisualConsequence | PS_TextureConsequence);
        REGISTER_PROPERTY_MEMORY(ColorCubeProjector, RGBA, offsetof(ColorCubeProjector, colors_[3]), RGBA(1, 1, 1), "Neg Y", "", PS_VisualConsequence | PS_TextureConsequence);
        REGISTER_PROPERTY_MEMORY(ColorCubeProjector, RGBA, offsetof(ColorCubeProjector, colors_[4]), RGBA(1, 1, 1), "Pos Z", "", PS_VisualConsequence | PS_TextureConsequence);
        REGISTER_PROPERTY_MEMORY(ColorCubeProjector, RGBA, offsetof(ColorCubeProjector, colors_[5]), RGBA(1, 1, 1), "Neg Z", "", PS_VisualConsequence | PS_TextureConsequence);

        context->RemoveProperty("ColorCubeProjector", "Material");
        context->RemoveProperty("ColorCubeProjector", "Image");
        context->RemoveProperty("ColorCubeProjector", "Mask");
    }

    BoundingBox ColorCubeProjector::ComputeBounds() const
    {
        BoundingBox bb(Vec3(-1, -1, -1), Vec3(1, 1, 1));
        bounds_ = bb;
        return bounds_;
    }

    void ColorCubeProjector::DrawDebug(IDebugRender* renderer, unsigned flags) const
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
            renderer->DrawDisc(GetWorldTransform() * Vec3(1, 0, 0), radius,  rotateMat * Vec3(1, 0, 0), colors_[0]);
            renderer->DrawDisc(GetWorldTransform() * Vec3(-1, 0, 0), radius, rotateMat * Vec3(-1, 0, 0), colors_[1]);
            renderer->DrawDisc(GetWorldTransform() * Vec3(0, 1, 0), radius,  rotateMat * Vec3(0, 1, 0), colors_[2]);
            renderer->DrawDisc(GetWorldTransform() * Vec3(0, -1, 0), radius, rotateMat * Vec3(0, -1, 0), colors_[3]);
            renderer->DrawDisc(GetWorldTransform() * Vec3(0, 0, 1), radius,  rotateMat * Vec3(0, 0, 1), colors_[4]);
            renderer->DrawDisc(GetWorldTransform() * Vec3(0, 0, -1), radius, rotateMat * Vec3(0, 0, -1), colors_[5]);
        }
    }

    RGBA ColorCubeProjector::SampleColorProjection(const Vec3& position, const Vec3& normal) const
    {
        auto trans = GetWorldTransform().Inverted();
        const Vec3 localPos = trans * position;
        const Vec3 localNormal = (trans.RotatePart() * normal).Normalized();

        BoundingBox bounds(Vec3(-1, -1, -1), Vec3(1, 1, 1));
        if (bounds.Contains(localPos))
        {
            auto nSquared = (localNormal * localNormal).Abs();
            RGBA ret = colors_[(localNormal.x < 0)] * nSquared.x + colors_[(localNormal.y < 0) + 2] * nSquared.y + colors_[(localNormal.z < 0) + 4] * nSquared.z;
            ret.a = 1.0f;
            //ret.Clip();
            //float len = sqrtf(ret.r * ret.r + ret.b * ret.b + ret.g * ret.g);
            //ret.r /= len;
            //ret.g /= len;
            //ret.b /= len;
            return ret;
        }

        return RGBA::Invalid;
    }

}