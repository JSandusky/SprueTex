#include <SprueEngine/Core/SplinePiece.h>

#include <SprueEngine/Core/Context.h>
#include <SprueEngine/Math/NearestIndex.h>

#include <SprueEngine/MathGeoLib/Geometry/LineSegment.h>

namespace SprueEngine
{

    SplinePiece::SplinePiece()
    {

    }

    SplinePiece::~SplinePiece()
    {

    }

    void SplinePiece::Register(Context* context)
    {
        context->RegisterFactory<SplinePiece>("SplinePiece", "Catmull-Rom curve that can be used to create spines, tentacles, or tails.");
        context->CopyBaseProperties("SkeletalPiece", "SplinePiece");

        REGISTER_PROPERTY_MEMORY(SplinePiece, float, offsetof(SplinePiece, boneSpacing_), 0.2f, "Bone Spacing", "Distance between bones generated along the spline", PS_Default);
        REGISTER_PROPERTY_MEMORY(SplinePiece, bool, offsetof(SplinePiece, markAsSpine_), true, "Mark as Spine", "Generated bones will be flagged as being 'spine' bodies", PS_Default);
    }

    unsigned SplinePiece::GetNearestSegmentIndex(const Vec3& pos, float segmentDelta) const
    {
        NearestIndex<unsigned> closest(0);
        unsigned steps = floorf(GetLength() / segmentDelta);
        float t = 0.0f;
        for (unsigned i = 0; i < steps; ++i, t += segmentDelta)
            closest.Check(LineSegment(GetValue(t), GetValue(t + segmentDelta)).DistanceSq(pos), i);
        return closest.closest_;
    }
}