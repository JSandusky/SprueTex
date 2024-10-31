#pragma once

#include <SprueEngine/ClassDef.h>
#include <SprueEngine/StringHash.h>
#include <SprueEngine/MathGeoLib/AllMath.h>

namespace SprueEngine
{
    class Joint;
    class Skeleton;
    class SceneObject;
    class SprueModel;

    class SPRUE SkeletonBuilder
    {
    public:
        SkeletonBuilder(const SprueModel* model);

        Skeleton* GetSkeleton() const { return skeleton_; }

    private:
        void Build(const SceneObject* object, Joint* currentJoint);
        void AddSkeleton(const StringHash& identifier, const Skeleton* skeleton, Joint* currentJoint);
        void AddJoint(const Joint* joint, Joint* currentJoint, const Mat3x4& transform, const Quat& rotateBy);

        Skeleton* skeleton_ = 0x0;
    };

}