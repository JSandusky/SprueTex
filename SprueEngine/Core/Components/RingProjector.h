#pragma once

#include <SprueEngine/Core/Components/TexturingComponent.h>

namespace SprueEngine
{
    /// Projects a texture cylindrically onto the mesh. Useful for creating straps, cuffs, etc.
    class SPRUE RingProjector : public TexturingComponent
    {
        BASECLASSDEF(RingProjector, TexturingComponent);
        NOCOPYDEF(RingProjector);
        SPRUE_EDITABLE(RingProjector);
    public:
        /// Construct.
        RingProjector();
        /// Destruct.
        virtual ~RingProjector();

        /// Register factory and properties.
        static void Register(Context*);

        virtual BoundingBox ComputeBounds() const override;

        virtual void DrawDebug(IDebugRender* renderer, unsigned flags) const override;
        virtual RGBA SampleColorProjection(const Vec3& position, const Vec3& normal) const override;

    private:
    };

}