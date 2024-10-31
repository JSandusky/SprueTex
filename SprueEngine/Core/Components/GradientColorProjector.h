#pragma once

#include <SprueEngine/Core/Components/TexturingComponent.h>

namespace SprueEngine
{

    /// Valve style ambient color cube.
    class SPRUE GradientColorProjector : public TexturingComponent
    {
        BASECLASSDEF(GradientColorProjector, TexturingComponent);
        NOCOPYDEF(GradientColorProjector);
        SPRUE_EDITABLE(GradientColorProjector);
    public:
        /// Construct.
        GradientColorProjector();
        /// Destruct.
        virtual ~GradientColorProjector();

        /// Register factory and properties.
        static void Register(Context* context);

        virtual BoundingBox ComputeBounds() const override;

        virtual void DrawDebug(IDebugRender* renderer, unsigned flags) const override;

        virtual RGBA SampleColorProjection(const Vec3& position, const Vec3& normal) const override;

    private:
        RGBA colors_[2];
        bool normalMode_ = true;
    };

}