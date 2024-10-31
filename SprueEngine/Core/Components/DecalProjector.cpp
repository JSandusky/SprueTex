#include "DecalProjector.h"

#include <SprueEngine/Core/Context.h>
#include <SprueEngine/IDebugRender.h>

namespace SprueEngine
{

    DecalProjector::DecalProjector() :
        TexturingComponent()
    {

    }

    DecalProjector::~DecalProjector()
    {

    }

    void DecalProjector::Register(Context* context)
    {
        context->RegisterFactory<DecalProjector>("DecalProjector", "");
        context->CopyBaseProperties("TexturingComponent", "DecalProjector");
        REGISTER_PROPERTY_MEMORY(DecalProjector, float, offsetof(DecalProjector, normalTolerance_), 0.25f, "Normal Tolerance", "Allowed deviation of the vertex normal to allow the texture to project there", PS_VisualConsequence | PS_TextureConsequence);
        REGISTER_PROPERTY_MEMORY(DecalProjector, bool, offsetof(DecalProjector, spraySplatMode_), false, "Particle Spray Mode", "Instead of projecting a single decal project multiple particles", PS_VisualConsequence | PS_TextureConsequence);
        REGISTER_PROPERTY_MEMORY(DecalProjector, unsigned, offsetof(DecalProjector, spraySplatCount_), 64, "Particle Count", "How many particles to emit when in particle spray mode", PS_VisualConsequence | PS_TextureConsequence);
        REGISTER_PROPERTY_MEMORY(DecalProjector, Vec2, offsetof(DecalProjector, particleSize_), Vec2(0.25f, 0.25f), "Particle Size", "Size of the splatted texture to use when in particle mode", PS_VisualConsequence | PS_TextureConsequence);
    }

    BoundingBox DecalProjector::ComputeBounds() const
    {
        Vec3 bottomLeft(-1, -0.1, -1);
        Vec3 bottomRight(1, -0.1, -1);
        Vec3 topLeft(-1, 0.1, 1);
        Vec3 topRight(1, 0.1, 1);
        BoundingBox bb(bottomLeft, bottomLeft);
        bb.Enclose(bottomRight);
        bb.Enclose(topLeft);
        bb.Enclose(topRight);
        bounds_ = bb;
        return bb;
    }

    void DecalProjector::DrawDebug(IDebugRender* renderer, unsigned flags) const
    {
        RGBA color = flags & SPRUE_DEBUG_HOVER ? RGBA::Gold : RGBA::Green;
        color.a = 0.75f;
        if (flags & SPRUE_DEBUG_PASSIVE)
            color.a = 0.1f;

        Vec3 bottomLeft(-1, 0, -1);
        Vec3 bottomRight(1, 0, -1);
        Vec3 topLeft(-1, 0, 1);
        Vec3 topRight(1, 0, 1);

        renderer->DrawLine(GetWorldTransform() * bottomLeft, GetWorldTransform() * bottomRight, color);
        renderer->DrawLine(GetWorldTransform() * bottomLeft, GetWorldTransform() * topLeft, color);
        renderer->DrawLine(GetWorldTransform() * topLeft, GetWorldTransform() * topRight, color);
        renderer->DrawLine(GetWorldTransform() * bottomRight, GetWorldTransform() * topRight, color);

        renderer->DrawLine(GetWorldTransform() * bottomLeft, GetWorldTransform() * (bottomLeft + Vec3(0, 2, 0)), RGBA::Blue * color.a);
        renderer->DrawLine(GetWorldTransform() * bottomRight, GetWorldTransform() * (bottomRight + Vec3(0, 2, 0)), RGBA::Blue * color.a);
        renderer->DrawLine(GetWorldTransform() * topLeft, GetWorldTransform() * (topLeft + Vec3(0, 2, 0)), RGBA::Blue * color.a);
        renderer->DrawLine(GetWorldTransform() * topRight, GetWorldTransform() * (topRight + Vec3(0, 2, 0)), RGBA::Blue * color.a);
    }

    RGBA DecalProjector::SampleColorProjection(const Vec3& position, const Vec3& normal) const
    {
        if (!GetImageData() || !GetImageData()->GetImage())
            return RGBA::Invalid;

        auto trans = GetWorldTransform().Inverted();
        const auto localPosition = trans * position;
        const auto localNormal = (trans.RotatePart() * normal).Normalized();
        if (localNormal.Dot(Vec3::NegativeY) < normalTolerance_)
            return RGBA::Invalid;

        Vec3 topLeft(-1, 0, 1);
        Plane plane(topLeft, Vec3::PositiveY);
        Vec3 planePos = plane.Project(localPosition.ToPos4());// plane.ClosestPoint(localPosition.ToPos4());
        
        Vec2 uvScale(1, 1);
        Vec2 uv = Vec2(localPosition.Dot(Vec3::PositiveX), localPosition.Dot(Vec3::PositiveZ));
        if (uv.x < -1.0 || uv.x > 1.0)
            return RGBA::Invalid;
        if (uv.y < -1.0 || uv.y > 1.0)
            return RGBA::Invalid;
        uv = NORMALIZE(uv, Vec2(-1, -1), Vec2(1, 1));
        uv *= uvScale;

        return GetImageData()->GetImage()->getBilinear(uv.x, uv.y);
    }
}