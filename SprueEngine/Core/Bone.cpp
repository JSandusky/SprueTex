#include "Bone.h"

#include <SprueEngine/Core/Context.h>

namespace SprueEngine
{

static const char* BONE_SKINNING_MODE_NAMES[] = {
    "Unskinned",
    "Envelope",
    "Automatic",
    0x0
};

Bone::Bone() : 
    base(),
    skinningMode_(BoneSkinningMode::BSM_Automatic)
{

}

Bone::~Bone()
{

}

void Bone::Register(Context* context)
{
    context->RegisterFactory<Bone>("Bone", "Animation bone the model will be skinned to");
    context->CopyBaseProperties(StringHash("SceneObject"), StringHash("Bone"));
    REGISTER_PROPERTY(Bone, float, GetGlowIntensity, SetGlowIntensity, 1.0f, "Weight Glow", "Illuminance intensity for automatic bone weights", PS_Default);
    REGISTER_ENUM(Bone, BoneSkinningMode, GetSkinningMode, SetSkinningMode, BoneSkinningMode::BSM_Automatic, "Skinning Mode", "How vertices will be weighted to bones (if at all)", PS_Default, BONE_SKINNING_MODE_NAMES);
    REGISTER_PROPERTY(Bone, bool, IsBlurringAllowed, SetBlurringAllowed, true, "Weight Blurring Allowed", "Controls whether this bone's weights in vertices can be adjusted by smoothing/blurring of vertex weights", PS_Default);
}

BoundingBox Bone::ComputeBounds() const
{
    return BoundingBox();
}

}