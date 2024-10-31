#pragma once

#include <SprueEngine/IEditable.h>

namespace SprueEngine
{
    extern const char* PaintLayerBlendModeNames[];
    enum PaintLayerBlendMode
    {
        PLBM_Normal,
        PLBM_Additive,
        PLBM_Subtract,
        PLBM_Multiply,
        PLBM_Divide,
        PLBM_ColorBurn,
        PLBM_LinearBurn,
        PLBM_Screen,
        PLBM_ColorDodge,
        PLBM_LinearDodge,
        PLBM_Dissolve
    };

    extern const char* PaintLayerChannelNames[];
    enum PaintLayerChannel
    {
        PLC_Albedo = 0,
        PLC_Roughness = 1,
        PLC_Glossiness = 2,
        PLC_SpecularColor = 3,
        PLC_Metalness = 4,
        PLC_Normal = 5,
        PLC_Height = 6,
    };

    class SPRUE TextureLayerGroup : public IEditable
    {
        NOCOPYDEF(TextureLayerGroup);
        BASECLASSDEF(TextureLayerGroup, IEditable);
        SPRUE_EDITABLE(TextureLayerGroup);
    public:
        TextureLayerGroup();
        virtual ~TextureLayerGroup();
        static void Register(Context*);

        std::string GetName() const { return name_; }
        void SetName(const std::string& name) { name_ = name; }

        bool IsEnabled() const { return enabled_; }
        void SetEnabled(bool state) { enabled_ = state; }

        std::vector<TextureLayerGroup*> GetChildren() { return children_; }

    private:
        std::vector<TextureLayerGroup*> children_;
        std::string name_;
        bool enabled_ = true;
    };

    /// A painting layer for pixel data
    class SPRUE TexturePaintLayer : public TextureLayerGroup
    {
        NOCOPYDEF(TexturePaintLayer);
        BASECLASSDEF(TexturePaintLayer, TextureLayerGroup);
        SPRUE_EDITABLE(TexturePaintLayer);
    public:
        /// Construct.
        TexturePaintLayer();
        /// Destruct.
        virtual ~TexturePaintLayer();

        /// Register factory and properties.
        static void Register(Context* context);

        PaintLayerBlendMode GetBlendMode() const { return blendMode_; }
        void SetBlendMode(PaintLayerBlendMode mode) { blendMode_ = mode; }

        PaintLayerChannel GetChannel() const { return channel_; }
        void SetChannel(PaintLayerChannel chan) { channel_ = chan; }

        float GetOpacity() const { return opacity_; }
        void SetOpacity(float value) { opacity_ = value; }

        std::shared_ptr<FilterableBlockMap<RGBA> > GetImage() { return image_; }

    private:
        float opacity_ = 1.0f;
        PaintLayerBlendMode blendMode_ = PaintLayerBlendMode::PLBM_Normal;
        PaintLayerChannel channel_ = PaintLayerChannel::PLC_Albedo;
        std::shared_ptr<FilterableBlockMap<RGBA> > image_;
    };

}