#include "Stdafx.h"

using namespace System::Runtime::InteropServices;

#include "Geometry.h"
#include "LaplaceProcessing.h"

#include <SprueEngine/Geometry/MeshData.h>
#include <SprueEngine/Geometry/Skeleton.h>
#include <SprueEngine/Geometry/LaplacianOperations.h>
#include <SprueEngine/ResponseCurve.h>

namespace SprueBindings
{

    MeshData^ LaplaceProcessing::Deform(MeshData^ target, List<LaplaceHandle^>^ controlPoints)
    {
        if (target != nullptr && target->GetInternalMeshData() != 0x0 && controlPoints != nullptr && controlPoints->Count > 0)
        {
            MeshData^ ret = gcnew MeshData(target->GetInternalMeshData()->CloneRaw());
            std::vector<std::pair<unsigned, SprueEngine::Vec3> > handles;
            for (int i = 0; i < controlPoints->Count; ++i)
            {
                auto ctrlPt = (*controlPoints)[i];
                handles.push_back(std::make_pair(
                    (unsigned)ctrlPt->Index, 
                    SprueEngine::Vec3(ctrlPt->Position.X, ctrlPt->Position.Y, ctrlPt->Position.Z
                )));
            }
            SprueEngine::LaplacianOperations::Deform(ret->GetInternalMeshData(), handles);
            return ret;
        }
        return nullptr;
    }
    
    void FillSkeleton(SprueEngine::Skeleton* target, PluginLib::SkeletonData^ source)
    {
        for (int i = 0; i < source->Inline->Count; ++i)
        {
            auto srcJoint = (*source->Inline)[i];
            SprueEngine::Joint* newJoint = new SprueEngine::Joint();
            newJoint->SetPosition(SprueEngine::Vec3(srcJoint->Position.X, srcJoint->Position.Y, srcJoint->Position.Z));
            newJoint->SetRotation(SprueEngine::Quat(srcJoint->Rotation.X, srcJoint->Rotation.Y, srcJoint->Rotation.Z, srcJoint->Rotation.W));
            newJoint->SetScale(SprueEngine::Vec3(1, 1, 1));// srcJoint->Scale.X, srcJoint->Scale.Y, srcJoint->Scale.Z));
            target->AddJoint(newJoint);
        }
        
        for (int i = 0; i < source->Inline->Count; ++i)
        {
            auto joint = (*source->Inline)[i];
            if (joint->Parent != nullptr)
            {
                int index = source->Inline->IndexOf(joint);
                int parentIndex = source->Inline->IndexOf(joint->Parent);
                target->GetAllJoints()[parentIndex]->GetChildren().push_back(target->GetAllJoints()[i]);
                target->GetAllJoints()[i]->SetParent(target->GetAllJoints()[parentIndex]);
            }
        }
    }

    bool LaplaceProcessing::CalculateBoneWeights(MeshData^ target, RCurve^ curve, BoneWeightCancel^ cancelFunc)
    {
        SprueEngine::ResponseCurve responseCurve;
        if (curve != nullptr)
        {
            responseCurve.type_ = (SprueEngine::CurveType)curve->CurveShape;
            responseCurve.xIntercept_ = curve->x;
            responseCurve.yIntercept_ = curve->y;
            responseCurve.slopeIntercept_ = curve->slope;
            responseCurve.exponent_ = curve->exp;
            responseCurve.flipX_ = curve->flipX;
            responseCurve.flipY_ = curve->flipY;
        }
        SprueEngine::BoneWeightCancelCallback callback = 0x0;
        if (cancelFunc != nullptr)
        {
            auto handle = GCHandle::Alloc(cancelFunc);
            auto ptr = Marshal::GetFunctionPointerForDelegate(cancelFunc);
            callback = static_cast<SprueEngine::BoneWeightCancelCallback>(ptr.ToPointer());

            if (target != nullptr && target->GetInternalMeshData() != 0x0 && target->Skeleton != nullptr)
            {
                std::auto_ptr<SprueEngine::Skeleton> skeleton = std::auto_ptr<SprueEngine::Skeleton>(new SprueEngine::Skeleton());
                FillSkeleton(skeleton.get(), target->Skeleton);
                if (skeleton->GetAllJoints().empty())
                {
                    handle.Free();
                    return false;
                }

                SprueEngine::LaplacianOperations::CalculateBoneWeightsV3(target->GetInternalMeshData(), skeleton.get(), responseCurve, callback);
                handle.Free();
                return true;
            }
        }
        else
        {
            if (target != nullptr && target->GetInternalMeshData() != 0x0 && target->Skeleton != nullptr)
            {
                std::auto_ptr<SprueEngine::Skeleton> skeleton = std::auto_ptr<SprueEngine::Skeleton>(new SprueEngine::Skeleton());
                FillSkeleton(skeleton.get(), target->Skeleton);
                if (skeleton->GetAllJoints().empty())
                    return false;

                SprueEngine::LaplacianOperations::CalculateBoneWeightsV3(target->GetInternalMeshData(), skeleton.get(), responseCurve, 0x0);
                return true;
            }
        }
        return false;
    }

}