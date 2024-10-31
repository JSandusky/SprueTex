#include "TexturingComponent.h"

#include <SprueEngine/Core/Context.h>

namespace SprueEngine
{

    static const char* TexCompMatName[] = {
        "Material",
        0x0
    };
    static const StringHash TexCompMatHash[] = {
        StringHash("Material")
    };

    TexturingComponent::TexturingComponent() : 
        Component() 
    { 
    }

    TexturingComponent::~TexturingComponent()
    {

    }

    void TexturingComponent::Register(Context* context)
    {
        context->CopyBaseProperties(StringHash("Component"), StringHash("TexturingComponent"));
        REGISTER_RESOURCE(TexturingComponent, BitmapResource, GetBitmapResourceHandle, SetBitmapResourceHandle, GetImageData, SetImageData, ResourceHandle("Image"), "Image", "", PS_VisualConsequence | PS_TextureConsequence);
        REGISTER_RESOURCE(TexturingComponent, BitmapResource, GetMaskResourceHandle, SetMaskResourceHandle, GetMaskData, SetMaskData, ResourceHandle("Image"), "Mask", "", PS_VisualConsequence | PS_TextureConsequence);
        REGISTER_EDITABLE(TexturingComponent, GetMaterialProperty, SetMaterialProperty, "Material", "", PS_IEditableObject | PS_VisualConsequence | PS_TextureConsequence, TexCompMatName, TexCompMatHash);
    }

    Variant TexturingComponent::GetMaterialProperty() const
    {
        return Variant((void*)&material_);
    }
 
    void TexturingComponent::SetMaterialProperty(Variant var)
    {
        // do nothing
    }

    Vec2 TexturingComponent::ProjectOntoTriangleUV(const Vec3& localPosition, const Vec3& localNormal, const Vec3* vertices, const Vec2* uvs, float normalTolerance) const
    {
#define INVALID_RESULT Vec2(FLT_MIN, FLT_MIN)
        if (localNormal.Dot(Vec3::NegativeY) < normalTolerance)
            return INVALID_RESULT;

        Triangle tri(vertices[0], vertices[1], vertices[2]);
        auto closestPoint = tri.PlaneCW().ClosestPoint(localPosition);
        auto baryCoords = tri.BarycentricUVW(closestPoint);

        if (!tri.BarycentricInsideTriangle(baryCoords))
            return INVALID_RESULT;

        return Vec2(
            uvs[0].x * baryCoords.x + uvs[1].x * baryCoords.y + uvs[2].x * baryCoords.z,
            uvs[0].y * baryCoords.x + uvs[1].y * baryCoords.y + uvs[2].x * baryCoords.z
        );
    }
}