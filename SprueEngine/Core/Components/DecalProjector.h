#pragma once

#include <SprueEngine/Core/Components/TexturingComponent.h>

namespace SprueEngine
{
    /// Projects a texture onto the generated mesh. The projection is planar against the projector.
    class SPRUE DecalProjector : public TexturingComponent
    {
        BASECLASSDEF(DecalProjector, TexturingComponent);
        NOCOPYDEF(DecalProjector);
        SPRUE_EDITABLE(DecalProjector);
    public:
        /// Construct.
        DecalProjector();
        /// Destruct.
        virtual ~DecalProjector();

        /// Bind factory and properties.
        static void Register(Context*);

        /// Does nothing.
        virtual BoundingBox ComputeBounds() const override;

        virtual void DrawDebug(IDebugRender* renderer, unsigned flags) const override;

        virtual RGBA SampleColorProjection(const Vec3& position, const Vec3& normal) const override;

    private:
        /// If true then we'll cast random rays at the surface. The rays WILL be checked against our mask if provided.
        bool spraySplatMode_ = false;
        /// If using spray-splat then this is how many rays will be cast.
        unsigned spraySplatCount_ = 64;
        /// How much the dotproduct of the surface normal and the ray may be deviate when using spray-splat.
        float normalTolerance_ = 0.25f;
        /// Splatting size
        Vec2 particleSize_ = Vec2(0.25f, 0.25f);
    };

}