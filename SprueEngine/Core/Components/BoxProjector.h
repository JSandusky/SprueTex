#pragma once

#include <SprueEngine/Core/Components/TexturingComponent.h>

namespace SprueEngine
{

    enum BoxProjectionMode
    {
        BPM_Box = 0,
        BPM_Triplanar = 1,
        BPM_Cubemap = 2
    };

    class SPRUE BoxProjector : public TexturingComponent
    {
        BASECLASSDEF(BoxProjector, TexturingComponent);
        NOCOPYDEF(BoxProjector);
        SPRUE_EDITABLE(BoxProjector);
    public:
        BoxProjector();
        virtual ~BoxProjector();

        static void Register(Context*);

        virtual BoundingBox ComputeBounds() const override;

        virtual void DrawDebug(IDebugRender* renderer, unsigned flags) const override;

        virtual RGBA SampleColorProjection(const Vec3& position, const Vec3& normal) const override;

    private:
        /// If true then we'll use triplanar instead of a most likelye garbage box projection.
        BoxProjectionMode mode_ = BPM_Triplanar;
        /// Whether to treat the alpha channel as a blend weight in triplanar mode.
        bool useAlphaAsHeight_ = false;
    };

}