#pragma once

#include <SprueEngine/Core/Components/TexturingComponent.h>

namespace SprueEngine
{
    /// Projects a texture onto the mesh following the strip. Use to create patterns such as embroidery.
    /// Supports optional head/tail ends to use special textures as well as using randomizing the internal texture.
    class SPRUE StripProjector : public TexturingComponent
    {
        BASECLASSDEF(StripProjector, TexturingComponent);
        NOCOPYDEF(StripProjector);
        SPRUE_EDITABLE(StripProjector);
    public:
        StripProjector();
        virtual ~StripProjector();

        static void Register(Context*);

        virtual BoundingBox ComputeBounds() const override;
        virtual RGBA SampleColorProjection(const Vec3& position, const Vec3& normal) const override;

        struct SPRUE Node
        {
            Vec3 position_ = Vec3(0, 0, 0);
            Quat rotation_ = Quat::identity;
            float width_ = 1.0f;
        };

        std::vector<Node*>& GetNodes() { return nodes_; }
        const std::vector<Node*>& GetNodes() const { return nodes_; }

        VariantVector GetPoints() const;
        void SetPoints(const VariantVector& pts);

        virtual void DrawDebug(IDebugRender* debug, unsigned flags = 0) const override;

    protected:
        virtual void GetGizmos(std::vector<std::shared_ptr<Gizmo> >& gizmos) override;

    private:
        class StripGizmo;
        std::vector<Node*> nodes_;
    };

}