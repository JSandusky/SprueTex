#include "SprueEngine/Core/SpaceDeformer.h"

#include "SprueEngine/Core/Context.h"

namespace SprueEngine
{

SpaceDeformer::SpaceDeformer()
{

}

SpaceDeformer::~SpaceDeformer()
{

}

void SpaceDeformer::Register(Context* context)
{
    /// TODO: add transform matrix
    context->RegisterFactory<SpaceDeformer>("SpaceDeformer", "Transforms space within in its' area of effect for scaling, offset, and twisting effects");
    context->CopyBaseProperties(StringHash("SceneObject"), StringHash("SpaceDeformer"));

    REGISTER_PROPERTY(SpaceDeformer, BoundingBox, GetBounds, SetBounds, BoundingBox(Vec3(-1, -1, -1), Vec3(1, 1, 1)), "Bounds", "", PS_Default);
    REGISTER_PROPERTY(SpaceDeformer, bool, IsBoneWarping, SetBoneWarping, false, "Warp Bones", "", PS_Default);
    REGISTER_PROPERTY_CONST_SET(SpaceDeformer, Vec3, GetOffset, SetOffset, Vec3(0, 0, 0), "Offset", "", PS_VisualConsequence);
    REGISTER_PROPERTY_CONST_SET(SpaceDeformer, Quat, GetRotationOffset, SetRotationOffset, Quat(), "Rotation Warp", "", PS_VisualConsequence);
    REGISTER_PROPERTY_CONST_SET(SpaceDeformer, Vec3, GetScaleAdjust, SetScaleAdjust, Vec3(0, 0, 0), "Scale Warp", "", PS_VisualConsequence);
    REGISTER_PROPERTY(SpaceDeformer, float, GetTwist, SetTwist, 0.0f, "Twist", "", PS_VisualConsequence);
    REGISTER_PROPERTY(SpaceDeformer, float, GetBend, SetBend, 0.0f, "Bend", "", PS_VisualConsequence);
}

void SpaceDeformer::Deform(Vec3& pos) const
{
    Vec3 trans(pos);
    trans = inverseWorldTransform_ * trans;
    trans = warpTrans_.Inverted() * trans;

    if (twist_ != 0)
        trans = Twist(trans, twist_);
    if (bend_ != 0)
        trans = CheapBend(trans, bend_);

    trans = worldTransform_ * trans;
    pos.x = trans.x;
    pos.y = trans.x;
    pos.z = trans.x;
}

void SpaceDeformer::UpdateWarpTransform()
{
    warpTrans_ = Mat3x4::FromTRS(offset_, rotation_, scale_);
}

}