#include "SkeletonBuilder.h"

#include <SprueEngine/Core/Bone.h>
#include <SprueEngine/Core/ModelPiece.h>
#include <SprueEngine/Geometry/Skeleton.h>
#include <SprueEngine/Core/SpinalPiece.h>
#include <SprueEngine/Core/SplinePiece.h>
#include <SprueEngine/Core/SprueModel.h>

namespace SprueEngine
{

    SkeletonBuilder::SkeletonBuilder(const SprueModel* model)
    {
        skeleton_ = new Skeleton();
        Joint* rootJoint = new Joint();
        rootJoint->SetPosition(model->GetWorldPosition());
        skeleton_->AddJoint(rootJoint);
        Build(model, rootJoint);
    }

    void SkeletonBuilder::Build(const SceneObject* object, Joint* currentJoint)
    {
        // Do not process disabled objects
        if (object->IsDisabled())
            return;

        // TODO check on world space vs local space for bones
        if (auto spinalPiece = dynamic_cast<const SpinalPiece*>(object))
        {
            auto bones = spinalPiece->GetVertebrae();
            std::vector<Joint*> newJoints(bones.size(), 0x0);
            for (unsigned i = 0; i < bones.size(); ++i)
            {
                Joint* newChild = new Joint();
                newJoints[i] = newChild;
                if (i == 0)
                    currentJoint->AddChild(newChild);
                else
                    newJoints[i - 1]->AddChild(newChild);

                newChild->SetModelSpacePosition(spinalPiece->GetWorldTransform() * bones[i]->pos_);

                //if (i != bones.size() - 1)
                //    newChild->SetRotation(Quat::LookAt(Vec3::PositiveZ, (bones[i + 1]->pos_ - bones[i]->pos_).Normalized(), Vec3::PositiveY, Vec3::PositiveY));
                //else
                //    newChild->SetRotation(Quat::LookAt(Vec3::PositiveZ, (bones[i]->pos_ - bones[i-1]->pos_).Normalized(), Vec3::PositiveY, Vec3::PositiveY));
            }

            // Find which joint we need to parent to based on nearest distance
            for (auto child : object->GetChildren())
            {
                if (!newJoints.empty())
                    Build(child, newJoints[spinalPiece->GetNearestVertebrae(child->GetPosition())]);
                else
                    Build(child, currentJoint);
            }
        }
        else if (auto splinePiece = dynamic_cast<const SplinePiece*>(object))
        {
            const float boneSpacing = splinePiece->GetBoneSpacing();
            const float splineLength = splinePiece->GetLength();
            const float ratio = splineLength / boneSpacing;
            const unsigned boneCt = floorf(ratio);
            std::vector<Joint*> newJoints(boneCt, 0x0);

            float curTime = 0.0f;
            for (unsigned i = 0; i < boneCt; ++i, curTime += ratio)
            {
                const Vec3 start = splinePiece->GetValue(curTime);
                const Vec3 end = splinePiece->GetValue(curTime + ratio);

                Joint* newJoint = new Joint();
                newJoint->SetPosition(start);
                newJoint->SetRotation(Quat::LookAt(Vec3::PositiveZ, (end - start).Normalized(), Vec3::PositiveY, Vec3::PositiveY));
                
                newJoints[i] = newJoint;
                currentJoint->AddChild(newJoint);
            }

            // Find which joint we need to parent to based on nearest distance
            for (auto child : object->GetChildren())
            {
                if (!newJoints.empty())
                    Build(child, newJoints[splinePiece->GetNearestSegmentIndex(child->GetPosition(), ratio)]);
                else
                    Build(child, currentJoint);
            }
        }
        else if (auto bone = dynamic_cast<const Bone*>(object))
        {
            Joint* newJoint = new Joint();
            newJoint->SetPosition(bone->GetWorldPosition());
            newJoint->SetRotation(bone->GetWorldRotation());
            currentJoint->AddChild(newJoint);
            for (auto child : object->GetChildren())
                Build(child, newJoint);
        }
        else if (auto modelPiece = dynamic_cast<const ModelPiece*>(object))
        {
            if (auto meshData = modelPiece->GetMeshData())
            {
                if (meshData->GetSkeleton() && meshData->GetSkeleton()->GetRootJoint())
                    AddSkeleton(modelPiece->GetMeshResourceHandle().Name, meshData->GetSkeleton(), currentJoint);
            }
        }
        else
        {
            for (auto child : object->GetChildren())
                Build(child, currentJoint);
        }
    }

    void SkeletonBuilder::AddSkeleton(const StringHash& identifier, const Skeleton* skeleton, Joint* currentJoint)
    {
        Joint* newRoot = new Joint();
        const Joint* srcRoot = skeleton->GetRootJoint();
        
        Mat3x4 transform = currentJoint->GetModelSpaceTransform();
        Quat rotation = transform.RotatePart().ToQuat();

        newRoot->SetPosition(srcRoot->GetPosition() * transform);
        newRoot->SetRotation(transform.RotatePart().ToQuat() * srcRoot->GetRotation());
        newRoot->SetSourceID(identifier);
        currentJoint->AddChild(newRoot);
        
        AddJoint(skeleton->GetRootJoint(), currentJoint, transform, rotation);
    }

    void SkeletonBuilder::AddJoint(const Joint* joint, Joint* currentJoint, const Mat3x4& transform, const Quat& rotateBy)
    {
        for (auto child : joint->GetChildren())
        {
            Joint* newJoint = new Joint();
            newJoint->AddChild(newJoint);

            newJoint->SetPosition(transform * child->GetPosition());
            newJoint->SetRotation(rotateBy * child->GetRotation());
            
            AddJoint(child, newJoint, transform, rotateBy);
        }
    }
}