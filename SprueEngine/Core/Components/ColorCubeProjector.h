#pragma once

#include <SprueEngine/Core/Components/TexturingComponent.h>

namespace SprueEngine
{

    /// Valve style ambient color cube.
    class SPRUE ColorCubeProjector : public TexturingComponent
    {
        BASECLASSDEF(ColorCubeProjector, TexturingComponent);
        NOCOPYDEF(ColorCubeProjector);
        SPRUE_EDITABLE(ColorCubeProjector);
    public:
        /// Construct.
        ColorCubeProjector();
        /// Destruct.
        virtual ~ColorCubeProjector();

        /// Register factory and properties.
        static void Register(Context* context);

        virtual BoundingBox ComputeBounds() const override;

        virtual void DrawDebug(IDebugRender* renderer, unsigned flags) const override;

        virtual RGBA SampleColorProjection(const Vec3& position, const Vec3& normal) const override;

    private:
        RGBA colors_[6];
    };

}