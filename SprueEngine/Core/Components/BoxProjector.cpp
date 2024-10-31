#include "BoxProjector.h"

#include <SprueEngine/Core/Context.h>
#include <SprueEngine/IDebugRender.h>

namespace SprueEngine
{

    static const char* BoxProjectionModeNames[] = {
        "Hard Box",
        "Triplanar",
        "Cubemap",
        0x0
    };

    BoxProjector::BoxProjector() :
        TexturingComponent()
    {

    }

    BoxProjector::~BoxProjector()
    {

    }

    void BoxProjector::Register(Context* context)
    {
        context->RegisterFactory<BoxProjector>("BoxProjector", "Uses a box volume to project a texture onto the target mesh");
        context->CopyBaseProperties("TexturingComponent", "BoxProjector");
        REGISTER_ENUM_MEMORY(BoxProjector, int, offsetof(BoxProjector, mode_), BPM_Triplanar, "Projection Mode", "Sets texture sampling mode", PS_VisualConsequence | PS_TextureConsequence, BoxProjectionModeNames);
        REGISTER_PROPERTY_MEMORY(BoxProjector, bool, offsetof(BoxProjector, useAlphaAsHeight_), false, "Use Alpha as Height", "Triplanar blending will be biased by treating alpha as a height/priority", PS_VisualConsequence | PS_TextureConsequence);
    }

    BoundingBox BoxProjector::ComputeBounds() const
    {
        BoundingBox bb(Vec3(-1, -1, -1), Vec3(1, 1, 1));
        bounds_ = bb;
        //bb.TransformAsAABB(GetWorldTransform());
        return bb;
    }

    void BoxProjector::DrawDebug(IDebugRender* renderer, unsigned flags) const
    {
        RGBA color = flags & SPRUE_DEBUG_HOVER ? RGBA::Gold : RGBA::Green;
        color.a = 0.75f;
        if (flags & SPRUE_DEBUG_PASSIVE)
            color.a = 0.1f;

        //BoundingBox bb(Vec3(-1, -1, -1), Vec3(1, 1, 1));
        //bb.TransformAsAABB(GetWorldTransform());
        renderer->DrawBoundingBox(GetWorldTransform(), BoundingBox(Vec3(-1, -1, -1), Vec3(1, 1, 1)), color);
    }

    RGBA BoxProjector::SampleColorProjection(const Vec3& position, const Vec3& normal) const
    {
        if (!GetImageData())
            return RGBA::Invalid;

        auto trans = GetWorldTransform().Inverted();
        const Vec3 localPos = trans * position;
        const Vec3 localNormal = (trans.RotatePart() * normal).Normalized();

        BoundingBox bounds(Vec3(-1, -1, -1), Vec3(1, 1, 1));
        if (bounds.Contains(localPos.ToPos4()))
        {
            if (mode_ == BPM_Triplanar)
            {
                Vec3 interpolatedNormal = localNormal.Abs();
                float weightSum = interpolatedNormal.x + interpolatedNormal.y + interpolatedNormal.z;
                interpolatedNormal /= weightSum;

                Vec3 scaling(1, 1, 1);

                Vec2 coord1 = Vec2(localPos.y, localPos.z) * scaling.x;
                Vec2 coord2 = Vec2(localPos.x, localPos.z) * scaling.y;
                Vec2 coord3 = Vec2(localPos.x, localPos.y) * scaling.z;

                RGBA col1 = GetImageData()->GetImage()->getBilinear(coord1.x / scaling.x, coord1.y / scaling.y);
                RGBA col2 = GetImageData()->GetImage()->getBilinear(coord2.x / scaling.x, coord2.y / scaling.y);
                RGBA col3 = GetImageData()->GetImage()->getBilinear(coord3.x / scaling.x, coord3.y / scaling.y);

                RGBA writeColor = col1 * interpolatedNormal.x + col2 * interpolatedNormal.y + col3 * interpolatedNormal.z;
                writeColor.a = 1.0f;

                return writeColor;
            }
            else if (mode_ == BPM_Cubemap)
            {
                const Vec3 absVec = localNormal.Abs();
                const int maxElem = absVec.MaxElementIndex();
                Vec2 coord;
                if (maxElem == 0)
                {
                    if (localNormal.x > 0)
                        coord.Set(((-localNormal.z / absVec.x) + 1) / 2.0f, ((-localNormal.y/absVec.x)+1)/2.0f);
                    else
                        coord.Set(((localNormal.z / absVec.x) + 1) / 2.0f, ((-localNormal.y / absVec.x) + 1) / 2.0f);
                }
                else if (maxElem == 1)
                {
                    if (localNormal.y > 0)
                        coord.Set(((localNormal.x / absVec.y) + 1) / 2.0f, ((localNormal.z / absVec.y) + 1) / 2.0f);
                    else
                        coord.Set(((localNormal.x / absVec.y) + 1) / 2.0f, ((-localNormal.z / absVec.y) + 1) / 2.0f);
                }
                else if (maxElem == 2)
                {
                    if (localNormal.z > 0)
                        coord.Set(((localNormal.x / absVec.z) + 1) / 2.0f, ((-localNormal.y / absVec.z) + 1) / 2.0f);
                    else
                        coord.Set(((-localNormal.x / absVec.z) + 1) / 2.0f, ((-localNormal.y / absVec.z) + 1) / 2.0f);
                }

                return GetImageData()->GetImage()->getBilinear(coord.x, coord.y);
            }
            else // BPM_Box
            {
                const Vec3 absVec = localNormal.Abs();
                const int maxElem = absVec.MaxElementIndex();
                Vec2 coord;

                // Normalized position indicates the sampling coordinates
                auto normalizedPosition = bounds.Normalized(localPos);

                if (maxElem == 0) // X axis
                    coord.Set(normalizedPosition.z, normalizedPosition.y);
                else if (maxElem == 1) // Y axis
                    coord.Set(normalizedPosition.x, normalizedPosition.z);
                else if (maxElem == 2) // Z axis
                    coord.Set(normalizedPosition.x, normalizedPosition.y);

                return GetImageData()->GetImage()->getBilinear(coord.x, coord.y);
            }
        }
        

        return RGBA::Invalid;
    }
}