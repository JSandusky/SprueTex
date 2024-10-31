#include "SprueEngine/Core/ModelPiece.h"

#include "SprueEngine/Core/Context.h"
#include "SprueEngine/Voxel/DistanceField.h"
#include "SprueEngine/IDebugRender.h"
#include "SprueEngine/Geometry/MeshData.h"
#include "SprueEngine/Geometry/Skeleton.h"
#include "SprueEngine/Math/Triangle.h"

namespace SprueEngine
{

static const char* ModelPieceMergeModeNames[] = {
    "Merge",
    "Independent",
    "CSG Add",
    "CSG Subtract",
    "CSG Intersect",
    "Clip then Merge",
    "Clip Independent",
    0x0
};

ModelPiece::ModelPiece() : base(),
    meshResource_(0x0)
{

}

ModelPiece::~ModelPiece()
{

}

void ModelPiece::Register(Context* context)
{
    context->RegisterFactory<ModelPiece>("ModelPiece", "Triangle mesh that can be either contoured, combined explicitly, or per-triangle CSG'd into the model");
    context->CopyBaseProperties(StringHash("SkeletalPiece"), StringHash("ModelPiece"));
    REGISTER_RESOURCE(ModelPiece, MeshResource, GetMeshResourceHandle, SetMeshResourceHandle, GetMeshData, SetMeshData, ResourceHandle("Mesh"), "Mesh", "", PS_VisualConsequence);
    REGISTER_ENUM(ModelPiece, ModelPieceMergeMode, GetMergeMode, SetMergeMode, MPMM_Merge, "Merge Method", "Determines how to combine this mesh into the aggregate model", PS_VisualConsequence, ModelPieceMergeModeNames);
    REGISTER_PROPERTY(ModelPiece, float, GetCSGSmoothing, SetCSGSmoothing, 0.5f, "CSG Smoothing", "If CSG is used the seams will be smoothed with this intensity (negative values will increase the seams", PS_VisualConsequence);
    REGISTER_PROPERTY(ModelPiece, bool, UseUVs, SetUseUVs, true, "Use UV Coords", "If the model contains UVs then those will be used instead of generating new UV coordinates", PS_Default);

    // Remove the shape mode property
    context->RemoveProperty("ModelPiece", "ShapeMode");
}

float ModelPiece::GetDensity(const Vec3& position) const
{
    if (meshResource_ == 0x0 || !IsMeshed())
        return std::numeric_limits<float>::max();

    //Vec3 samplePoint = GetWorldTransform().Inverse() * position;
    //float current = std::numeric_limits<float>::max();
    //for (unsigned i = 0; i < meshResource_->GetMeshCount(); ++i)
    //{
    //    if (meshResource_->GetMesh(i)->IsVoxelized())
    //        current = SprueMin(current, meshResource_->GetMesh(i)->GetDistanceField()->GetDistance(samplePoint.x, samplePoint.y, samplePoint.z, true));
    //}
    //
    //return current;
    return FLT_MAX / 2.0f;
}

BoundingBox ModelPiece::ComputeBounds() const
{
    if (meshResource_)
    {
        for (unsigned i = 0; i < meshResource_->GetMeshCount(); ++i)
        {
            if (i == 0)
                bounds_ = meshResource_->GetMesh(i)->GetBounds();
            else
                bounds_.Enclose(meshResource_->GetMesh(i)->GetBounds());
        }
        return bounds_;
    }
    return bounds_ = BoundingBox();
}

unsigned ModelPiece::CalculateStructuralHash() const
{
    // cast pointer to uint?
    return meshResource_ ? (unsigned)meshResource_.get() * (mode_ * 32) : 0;
}

bool ModelPiece::TestRayAccurate(const Ray& ray, IntersectionInfo* info) const
{
    float dist2 = FLT_MAX;
    if (meshResource_ && TestRayFast(ray, 0x0))
    {
        Ray transformedRay = GetWorldTransform().Inverted() * ray;
        for (MeshData* mesh : meshResource_->GetMeshes())
        {
            if (mesh->GetBounds().Intersects(transformedRay))
            {
                for (unsigned i = 0; i < mesh->indexBuffer_.size(); i += 3)
                {
                    math::Triangle tri(mesh->positionBuffer_[mesh->indexBuffer_[i]].ToPos4(),
                        mesh->positionBuffer_[mesh->indexBuffer_[i + 1]].ToPos4(), 
                        mesh->positionBuffer_[mesh->indexBuffer_[i + 2]].ToPos4());

                    //if (!ray.Intersects(tri.GetBounds()))
                    //    continue;

                    float thisDist = FLT_MAX;
                    vec thisHit;
                    if (transformedRay.Intersects(tri, &thisDist, &thisHit))
                    //if (tri.IntersectRay(ray, &thisDist, &thisHit) && thisDist < dist2)
                    {
                        dist2 = thisDist;
                        if (info)
                        {
                            info->parent = const_cast<SceneObject*>(GetParent());
                            info->object = const_cast<ModelPiece*>(this);
                            info->t = thisDist;
                            info->hit = thisHit;
                        }
                    }
                }
            }
        }
    }
    if (dist2 < FLT_MAX)
        return true;
    return false;
}

void ModelPiece::DrawDebug(IDebugRender* renderer, unsigned flags) const
{
    if (meshResource_ && meshResource_->GetMeshCount())
    {
        RGBA color = flags & SPRUE_DEBUG_HOVER ? RGBA::Gold : RGBA::Green;
        color.a = 0.75f;
        if (flags & SPRUE_DEBUG_PASSIVE)
            color.a = 0.1f;
        for (unsigned i = 0; i < meshResource_->GetMeshCount(); ++i)
        {
            MeshData* meshData = meshResource_->GetMesh(i);
            for (unsigned t = 0; t < meshData->indexBuffer_.size(); t += 3)
            {
                const unsigned a = meshData->indexBuffer_[t];
                const unsigned b = meshData->indexBuffer_[t+1];
                const unsigned c = meshData->indexBuffer_[t+2];
                renderer->DrawLine(GetWorldTransform() * meshData->positionBuffer_[a], GetWorldTransform() * meshData->positionBuffer_[b], color);
                renderer->DrawLine(GetWorldTransform() * meshData->positionBuffer_[b], GetWorldTransform() * meshData->positionBuffer_[c], color);
                renderer->DrawLine(GetWorldTransform() * meshData->positionBuffer_[c], GetWorldTransform() * meshData->positionBuffer_[a], color);
            }
        }

        if (auto skeleton = meshResource_->GetSkeleton())
            skeleton->DrawDebug(renderer, GetWorldTransform());
    }
}

}