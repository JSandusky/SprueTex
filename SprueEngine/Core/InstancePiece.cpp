#include "SprueEngine/Core/InstancePiece.h"

#include "SprueEngine/Core/Context.h"
#include "SprueEngine/IDebugRender.h"

namespace SprueEngine
{

InstancePiece::InstancePiece() : base(),
    referencePiece_(0x0)
{

}

InstancePiece::InstancePiece(SpruePiece* refPiece) : base(),
    referencePiece_(refPiece)
{

}

InstancePiece::~InstancePiece()
{
}

void InstancePiece::Register(Context* context)
{
    context->RegisterFactory<InstancePiece>("InstancePiece", "References another Sprue model that is not explicitly included in the scene, functions as a link");
    context->CopyBaseProperties(StringHash("SpruePiece"), StringHash("InstancePiece"));
}

float InstancePiece::GetDensity(const Vec3& position) const
{
    const Vec3 samplePos = GetWorldTransform().Inverse() * position;

    if (referencePiece_ != 0x0)
        return referencePiece_->CalculateDensity(samplePos);

    return 0.0f;
}

void InstancePiece::SetReferencePiece(SpruePiece* newPiece)
{
    referencePiece_ = newPiece;
}

BoundingBox InstancePiece::ComputeBounds() const
{
    BoundingBox bounds;

    if (referencePiece_)
        bounds = referencePiece_->ComputeBounds();
    return bounds_ = bounds;
}

bool InstancePiece::TestRayAccurate(const Ray& ray, IntersectionInfo* info) const
{
    if (referencePiece_ && TestRayFast(ray, 0x0))
        return referencePiece_->TestRayAccurate(ray, info);
    return false;
}

void InstancePiece::DrawDebug(IDebugRender* renderer, unsigned flags) const
{
    if (referencePiece_)
        referencePiece_->DrawDebug(renderer, flags);
}

}