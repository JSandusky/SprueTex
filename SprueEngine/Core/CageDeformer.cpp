#include "CageDeformer.h"

#include <SprueEngine/Core/Context.h>
#include <SprueEngine/GeneralUtility.h>
#include <SprueEngine/Math/MathDef.h>
#include <SprueEngine/MathGeoLib/AllMath.h>

#include <limits>

namespace SprueEngine
{

struct CageDeformer::CageVertexGizmo : public Gizmo
{
    Vec3* target;
    CageVertexGizmo(CageDeformer* cage, Vec3* target, bool deform) : Gizmo(cage, GIZ_Translate | (deform ? GIZ_DeformedControlPoint : GIZ_InitialControlPoint)), target(target)
    {
        transform_ = Mat3x4::FromTRS(cage->GetWorldTransform() * (*target), Quat(), Vec3::one);
    }

    virtual bool UpdateValue() override
    {
        CageDeformer* cage = (CageDeformer*)source_;
        Mat3x4 inverseMat = cage->GetWorldTransform().Inverted();

        *target = inverseMat * transform_.TranslatePart();

        return true;
    }

    virtual void RefreshValue() override
    {
        CageDeformer* cage = (CageDeformer*)source_;
        transform_ = Mat3x4::FromTRS(cage->GetWorldTransform() * (*target), Quat(), Vec3::one);
    }

    virtual bool Equals(Gizmo* rhs) const override
    {
        if (auto other = dynamic_cast<CageVertexGizmo*>(rhs))
            return other->source_ == source_; //TODO: not good enough
        return false;
    }
};

void CageDeformer::Register(Context* context)
{
    context->RegisterFactory<CageDeformer>("CageDeformer", "Uses a lattice cage to deform space");
    context->CopyBaseProperties(StringHash("SceneObject"), StringHash("CageDeformer"));
}

bool SameSide(const Vec3& v1, const Vec3& v2, const Vec3& v3, const Vec3& v4, const Vec3& p)
{
    Vec3 normal = (v2 - v1).Cross(v3 - v1).Normalized();
    float dotV4 = normal.Dot((v4 - v1).Normalized());
    float dotP = normal.Dot((p - v1).Normalized());
    return sgn(dotV4) == sgn(dotP);
}

bool PointInTetrahedron(const Vec3& v1, const Vec3& v2, const Vec3& v3, const Vec3& v4, const Vec3& p)
{
    return SameSide(v1, v2, v3, v4, p) &&
        SameSide(v2, v3, v4, v1, p) &&
        SameSide(v3, v4, v1, v2, p) &&
        SameSide(v4, v1, v2, v3, p);
}

bool CageDeformer::Tetrahedron::Contains(const CageDeformer* pointSource, const Vec3& pt) const
{
    return PointInTetrahedron(
        pointSource->initialRig_[Indices[0]],
        pointSource->initialRig_[Indices[1]],
        pointSource->initialRig_[Indices[2]],
        pointSource->initialRig_[Indices[3]],
        pt);
}

void CageDeformer::Tetrahedron::CalculateWeights(const CageDeformer* pointSource, const Vec3& pt, float* weights) const
{
    for (unsigned i = 0; i < 4; ++i)
        weights[i] = (pointSource->initialRig_[Indices[i]] - pt).LengthSq();
}

CageDeformer::CageDeformer() :
    gridSizeX_(0),
    gridSizeY_(0),
    gridSizeZ_(0)
{

}

CageDeformer::~CageDeformer()
{

}

#define CAGE_POINT_COUNT 4
void CageDeformer::Deform(Vec3& pos) const
{
    if (gridSizeX_ < 2 || gridSizeY_ < 2 || gridSizeZ_ < 2)
        return;

    ComputeTetrahedra();

    Vec3 pt(pos);
    pt = inverseWorldTransform_ * pt;

    // Compute dist2 for this point to all control points
    // OLD const unsigned ptCt = GetNumberOfPoints();
    // OLD std::vector<float> weights(ptCt);
    // OLD for (unsigned i = 0; i < ptCt; ++i)
    // OLD {
    // OLD     float weight = (pt - initialRig_[i]).Length2();
    // OLD     weights[i] = weight;
    // OLD }
    // OLD 
    // OLD // Sort for the closest vertices
    // OLD std::vector<size_t> indices = sort_indexes_less<float>(weights);
    // OLD 
    // OLD float bestWeights[CAGE_POINT_COUNT];
    // OLD Vec3 initialPoints[CAGE_POINT_COUNT];
    // OLD Vec3 deformedPoints[CAGE_POINT_COUNT];
    // OLD for (unsigned i = 0; i < CAGE_POINT_COUNT; ++i)
    // OLD {
    // OLD     bestWeights[i] = weights[indices[i]];
    // OLD     initialPoints[i] = initialRig_[indices[i]];
    // OLD     deformedPoints[i] = deformedRig_[indices[i]];
    // OLD }
    // OLD 
    // OLD // If this point was not inside of our initial rig, then we're not going to deform
    // OLD if (!PointInTetrahedron(initialPoints[0], initialPoints[1], initialPoints[2], initialPoints[3], pt))
    // OLD     return;
    // OLD 
    // OLD // Equalize our best weights
    // OLD InvertedNormalize(bestWeights, CAGE_POINT_COUNT);
    // OLD 
    // OLD Vec3 displacement;
    // OLD for (unsigned i = 0; i < CAGE_POINT_COUNT; ++i)
    // OLD     displacement += (deformedPoints[i] - initialPoints[i]) * bestWeights[i];

    float weights[4] = { 0.0f, 0.0f, 0.0f, 0.0f };
    unsigned indices[4] = { -1, -1, -1, -1 };
    for (unsigned i = 0; i < tetrahedrons_.size(); ++i)
    {
        if (tetrahedrons_[i].Contains(this, pt))
        {
            tetrahedrons_[i].CalculateWeights(this, pt, weights);
            memcpy(indices, tetrahedrons_[i].Indices, sizeof(unsigned) * 4);
            break;
        }
    }

    if (Sum(weights, 4) <= 0.0f)
        return;

    InvertedNormalize(weights, 4);

    Vec3 displacement;
    for (unsigned i = 0; i < 4; ++i)
        displacement += (deformedRig_[indices[i]] - initialRig_[indices[i]]) * weights[i];
    
    pt += displacement;
    pt = GetWorldTransform() * pt;

    pos.x = pt.x;
    pos.y = pt.y;
    pos.z = pt.z;
}

void CageDeformer::ComputeTetrahedra() const
{
    if (tetrahedrons_.size() != GetCellCount())
    {
        tetrahedrons_.resize(GetCellCount());

        unsigned tetIndex = 0;
        for(unsigned xx = 0; xx < gridSizeX_ - 1; ++xx)
        {
            for (unsigned yy = 0; yy < gridSizeY_ - 1; ++yy)
            {
                for (unsigned zz = 0; zz < gridSizeZ_ - 1; ++zz)
                {
                    /*
                    C (TLB)     D (TRB)
                       ___________
                      /|         /|
                    A/_|________/B| (TLF / TRF)
                    |  |        | |
                    | G|________|_| H (BLB, BRB)
                    | /         | /
                    |/__________|/
                    E           F (BLF / BRF)
                    */
                    int bottomLeftFront = ToArrayIndex(xx, yy, zz, gridSizeX_, gridSizeY_, gridSizeZ_);
                    int bottomRightFront = ToArrayIndex(xx + 1, yy, zz, gridSizeX_, gridSizeY_, gridSizeZ_);
                    int bottomLeftBack = ToArrayIndex(xx, yy, zz + 1, gridSizeX_, gridSizeY_, gridSizeZ_);
                    int bottomRightBack = ToArrayIndex(xx + 1, yy, zz + 1, gridSizeX_, gridSizeY_, gridSizeZ_);

                    int topLeftFront = ToArrayIndex(xx, yy + 1, zz, gridSizeX_, gridSizeY_, gridSizeZ_);
                    int topRightFront = ToArrayIndex(xx + 1, yy + 1, zz, gridSizeX_, gridSizeY_, gridSizeZ_);
                    int topLeftBack = ToArrayIndex(xx, yy + 1, zz + 1, gridSizeX_, gridSizeY_, gridSizeZ_);
                    int topRightBack = ToArrayIndex(xx + 1, yy + 1, zz + 1, gridSizeX_, gridSizeY_, gridSizeZ_);

                    // Given the above cube the 5 tetrahedra are:
                    // ACDG = topLeftFront, topLeftBack, topRightBack, bottomLeftBack(Checked)
                    // ABDF = topLeftFront, topRightFront, topRightBack, bottomRightFront(Checked)
                    // EFGA = bottomLeftFront, bottomRightFront, bottomLeftBack, topLeftFront(Checked)
                    // FHGD = bottomRightFront, bottomRightBack, bottomLeftBack, topRightBack(Checked)
                    // ADGF = topLeftFront, topRightBack, bottomLeftBack, bottomRightFront(Checked)

                    tetrahedrons_[tetIndex++].Set(topLeftFront, topLeftBack, topRightBack, bottomLeftBack);
                    tetrahedrons_[tetIndex++].Set(topLeftFront, topRightFront, topRightBack, bottomRightFront);
                    tetrahedrons_[tetIndex++].Set(bottomLeftFront, bottomRightFront, bottomLeftBack, topLeftFront);
                    tetrahedrons_[tetIndex++].Set(bottomRightFront, bottomRightBack, bottomLeftBack, topRightBack);
                    tetrahedrons_[tetIndex++].Set(topLeftFront, topRightBack, bottomLeftBack, bottomRightFront);
                }
            }
        }
    }
}

BoundingBox CageDeformer::ComputeBounds() const
{
    BoundingBox bounds;

    for (auto vert : initialRig_)
        bounds.Enclose(vert);
    for (auto vert : deformedRig_)
        bounds.Enclose(vert);

    return bounds_ = bounds;
}

void CageDeformer::GetGizmos(std::vector<std::shared_ptr<Gizmo> >& gizmos)
{
    base::GetGizmos(gizmos);
    for (auto vert : initialRig_)
        gizmos.push_back(std::shared_ptr<Gizmo>(new CageVertexGizmo(this, &vert, false)));
    for (auto vert : deformedRig_)
        gizmos.push_back(std::shared_ptr<Gizmo>(new CageVertexGizmo(this, &vert, true)));
}

}