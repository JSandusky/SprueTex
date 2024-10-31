#include "TexturePaintLayer.h"

#include <SprueEngine/Core/Context.h>

namespace SprueEngine
{
    const char* PaintLayerBlendModeNames[] = {
        "Normal",
        "Additive",
        "Subtract",
        "Multiply",
        "Divide",
        "Color Burn",
        "Linear Burn",
        "Screen",
        "ColorDodge",
        "LinearDodge",
        "Dissolve",
        0x0
    };

    const char* PaintLayerChannelNames[] = {
        "Albedo",
        "Roughness",
        "Glossiness",
        "Specular Color",
        "Metalness",
        "Normal",
        "Height",
        0x0
    };

    TextureLayerGroup::TextureLayerGroup()
    {

    }

    TextureLayerGroup::~TextureLayerGroup()
    {

    }

    void TextureLayerGroup::Register(Context* context)
    {
        context->RegisterFactory<TextureLayerGroup>("TextureLayerGroup", "Arranges layers into groups for easier management");
        REGISTER_PROPERTY_CONST_SET(TextureLayerGroup, std::string, GetName, SetName, std::string(), "Name", "", PS_Default);
        REGISTER_PROPERTY(TextureLayerGroup, bool, IsEnabled, SetEnabled, true, "Is Enabled", "Controls whether the layer will be processed or not", PS_IsVisibility);
    }

    TexturePaintLayer::TexturePaintLayer()
    {

    }

    TexturePaintLayer::~TexturePaintLayer()
    {

    }

    void TexturePaintLayer::Register(Context* context)
    {
        context->RegisterFactory<TexturePaintLayer>("TexturePaintLayer", "A raster painting layer for texture application");
        REGISTER_PROPERTY_CONST_SET(TexturePaintLayer, std::string, GetName, SetName, std::string(), "Name", "", PS_Default);
        REGISTER_PROPERTY(TexturePaintLayer, bool, IsEnabled, SetEnabled, true, "Is Enabled", "Controls whether the layer will be processed or not", PS_IsVisibility);
        REGISTER_ENUM(TexturePaintLayer, PaintLayerBlendMode, GetBlendMode, SetBlendMode, PLBM_Normal, "Blend Mode", "Controls how this layer will blend with other layers on the same channel", PS_VisualConsequence, PaintLayerBlendModeNames);
        REGISTER_ENUM(TexturePaintLayer, PaintLayerChannel, GetChannel, SetChannel, PLC_Albedo, "Channel", "Specifies which output texture this layer belongs to", PS_VisualConsequence, PaintLayerChannelNames);
        REGISTER_PROPERTY(TexturePaintLayer, float, GetOpacity, SetOpacity, 1.0f, "Opacity", "Defines the blending power with which this layer will blend against other layers on the same channel", PS_VisualConsequence | PS_NormalRange);
    }

}