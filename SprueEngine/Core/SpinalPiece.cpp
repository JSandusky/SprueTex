#include <SprueEngine/Core/SpinalPiece.h>

#include <SprueEngine/Core/Context.h>
#include <SprueEngine/Core/DensityFunctions.h>
#include <SprueEngine/IDebugRender.h>
#include <SprueEngine/Math/NearestIndex.h>

#include <SprueEngine/MathGeoLib/Geometry/LineSegment.h>
#include <SprueEngine/MathGeoLib/Math/Quat.h>
#include <SprueEngine/MathGeoLib/Math/float3x4.h>
#include <SprueEngine/MathGeoLib/Math/float3.h>
#include <SprueEngine/MathGeoLib/Math/float4.h>

namespace SprueEngine
{

#define POINT_ON_SPHERE(sphere, theta, phi) Vec3( \
        sphere.pos.x + sphere.r * sinf((float)(theta)  * DEG_TO_RAD) * sinf((float)(phi) * DEG_TO_RAD), \
        sphere.pos.y + sphere.r * cosf((float)(phi) * DEG_TO_RAD), \
        sphere.pos.z + sphere.r * cosf((float)(theta) * DEG_TO_RAD) * sinf((float)(phi) * DEG_TO_RAD))

struct SpinalPiece::SpinalGizmo : public Gizmo
{
    unsigned index_;
    Vertebrae* vert_;

    SpinalGizmo(SpinalPiece* spine, unsigned index) : 
        Gizmo(spine, GIZ_Translate | GIZ_Scale | GIZ_InitialControlPoint),
        index_(index)
    {
        gizmoId_ = index + 1;
        vert_ = spine->vertebrae_[index];
        transform_ = Mat3x4::FromTRS(spine->GetWorldTransform() * vert_->pos_, Quat(), vert_->dim_);
    }

    virtual bool UpdateValue() override
    {
        SpinalPiece* spine = (SpinalPiece*)source_;
        Mat3x4 inverse = spine->GetWorldTransform().Inverted();

        vert_->pos_ = inverse * transform_.TranslatePart();
        vert_->dim_ = transform_.GetScale();

        return true;
    }

    virtual void RefreshValue() override
    {
        SpinalPiece* spine = (SpinalPiece*)source_;
        transform_ = Mat3x4::FromTRS(spine->GetWorldTransform() * vert_->pos_, Quat(), vert_->dim_);
    }

    virtual void GetCommands(std::vector<unsigned>& commandIDs) override
    {
        commandIDs.push_back(SPRUE_GIZ_CMD_INSERT_AFTER);
        commandIDs.push_back(SPRUE_GIZ_CMD_INSERT_BEFORE);
        commandIDs.push_back(SPRUE_GIZ_CMD_REMOVE);
    }

    virtual bool ExecuteCommand(unsigned commandID, const Variant& commandParam) override
    {
        if (commandID == SPRUE_GIZ_CMD_INSERT_AFTER)
        {
            if (SpinalPiece* spine = (SpinalPiece*)source_)
            {
                Vertebrae* v = new Vertebrae();
                Vertebrae* current = *(spine->vertebrae_.begin() + index_);
                Vertebrae* next = 0x0;
                if (index_ < spine->vertebrae_.size() - 1)
                    next = *(spine->vertebrae_.begin() + index_ + 1);

                if (next)
                {
                    v->pos_ = Lerp(current->pos_, next->pos_, 0.5f);
                    v->dim_ = Lerp(current->dim_, next->dim_, 0.5f);
                }
                else
                {
                    next = *(spine->vertebrae_.begin() + index_ - 1);
                    if (next)
                    {
                        v->pos_ = current->pos_ + (current->pos_ - next->pos_);
                        v->dim_ = current->dim_;
                    }
                    else
                    {
                        v->pos_ = current->pos_ + Vec3(1, 1, 1);
                        v->dim_ = current->dim_;
                    }
                }

                if (index_ == spine->vertebrae_.size() - 1)
                    spine->vertebrae_.push_back(v);
                else
                    spine->vertebrae_.insert(spine->vertebrae_.begin() + index_ + 1, v);

                return true;
            }
        }
        else if (commandID == SPRUE_GIZ_CMD_INSERT_BEFORE)
        {
            if (SpinalPiece* spine = (SpinalPiece*)source_)
            {
                Vertebrae* v = new Vertebrae();
                Vertebrae* current = *(spine->vertebrae_.begin() + index_);
                Vertebrae* previous = 0x0;
                if (index_ > 0)
                    previous = *(spine->vertebrae_.begin() + index_ - 1);

                if (previous)
                {
                    v->pos_ = Lerp(previous->pos_, current->pos_, 0.5f);
                    v->dim_ = Lerp(previous->dim_, current->dim_, 0.5f);
                }
                else
                {
                    delete v;
                    return false;
                }

                spine->vertebrae_.insert(spine->vertebrae_.begin() + index_, v);

                return true;
            }
        }
        else if (commandID == SPRUE_GIZ_CMD_REMOVE)
        {
            if (SpinalPiece* spine = (SpinalPiece*)source_)
            {
                if (spine->vertebrae_.size() <= 2)
                    return false;
                Vertebrae* erased = *(spine->vertebrae_.begin() + index_);
                spine->vertebrae_.erase(spine->vertebrae_.begin() + index_);
                delete erased;
                return true;
            }
        }
        return false;
    }

    virtual bool Equals(Gizmo* rhs) const override
    {
        if (auto other = dynamic_cast<SpinalGizmo*>(rhs))
            return other->source_ == source_ && other->vert_ == vert_;
        return false;
    }
};

SpinalPiece::SpinalPiece() : base()
{
    Vertebrae* v = new Vertebrae();
    v->pos_ = Vec3(0, 0, 0);
    v->dim_ = Vec3(2, 2, 2);
    vertebrae_.push_back(v);

    v = new Vertebrae();
    v->pos_ = Vec3(0, 3, 5);
    v->dim_ = Vec3(3, 3, 3);
    vertebrae_.push_back(v);

    v = new Vertebrae();
    v->pos_ = Vec3(0, 7, 15);
    v->dim_ = Vec3(4, 4, 4);
    vertebrae_.push_back(v);
}

SpinalPiece::~SpinalPiece()
{

}

void SpinalPiece::Register(Context* context)
{
    context->RegisterFactory<SpinalPiece>("SpinalPiece", "Multi-joint tube that can be used for body trunks, tails, etc");
    context->CopyBaseProperties(StringHash("SkeletalPiece"), StringHash("SpinalPiece"));
    REGISTER_PROPERTY_CONST_SET(SpinalPiece, VariantVector, GetPoints, SetPoints, VariantVector(), "Points", "Secret list of points", PS_Secret);
    REGISTER_PROPERTY(SpinalPiece, float, GetBoneMinLength, SetBoneMinLength, 0.2f, "Minimum Bone Length", "Used in automatic pruning of bones to discard some bones based on length constraints", PS_Default);
    REGISTER_PROPERTY(SpinalPiece, float, GetAutoInsertionThreshold, SetAutoInsertionThreshold, 0.1f, "Autoinsertion Threshold", "How much distance needs to be coverted before a bone will automatically be inserted, zero or less for none", PS_Default);
    REGISTER_PROPERTY_MEMORY(SpinalPiece, bool, offsetof(SpinalPiece, markAsSpine_), true, "Mark As Spine", "All contained bones will be flagged as being 'Spine' bodies when the model is generated", PS_Default);
}

#define RAY_EPSILON 0.001f
#define MAX_RAY_STEPS 64
bool SpinalPiece::TestRayAccurate(const Ray& ray, IntersectionInfo* info) const
{
    // Check if we can even collide
    if (vertebrae_.size())// && TestRayFast(ray, 0x0))
    {
        Ray testRay = ray;
        testRay.Transform(GetWorldTransform().Inverted());
        float t = 0.0f;
        for (int i = 0; i < MAX_RAY_STEPS; ++i) {
            const Vec3 testPoint = testRay.pos + testRay.dir * t;
            float d = GetDensity(testPoint);
            if (d < RAY_EPSILON) {
                if (info)
                {
                    info->hit = testPoint;
                    info->parent = const_cast<SceneObject*>(GetParent());
                    info->object = const_cast<SpinalPiece*>(this);
                    info->t = (testPoint - ray.pos).LengthSq();
                }
                return true;
            }
            t += d;
        }
    }

    return false;
}

float SpinalPiece::GetDensity(const Vec3& position) const
{
    const Vec3 samplePos = GetWorldTransform().Inverse() * position;

    float currentDistance = 100000.0f;
    if (vertebrae_.empty())
        return currentDistance;

    for (int i = 0; i < ((int)vertebrae_.size()) - 1; ++i)
    {
        Vec3 pa = samplePos - vertebrae_[i]->pos_;
        Vec3 ba = (vertebrae_[i + 1]->pos_ - vertebrae_[i]->pos_);
        float h = CLAMP(pa.Dot(ba) / ba.Dot(ba), 0.0, 1.0);

        // Calculate an ellipsoid distance at the given point
        Vec3 interpRad = SprueLerp(vertebrae_[i]->dim_, vertebrae_[i + 1]->dim_, h);
        const float distance = (((pa - ba*h) / interpRad).Length() - 1.0f) * SprueMin(interpRad.x, SprueMin(interpRad.y, interpRad.z));

        currentDistance = SprueMin(currentDistance, distance);
    }

    return currentDistance;
}

/// Add a vertebrae to the front
SpinalPiece::Vertebrae* SpinalPiece::AddHeadBone()
{
    Vertebrae* newVert = new Vertebrae();
    Vec3 difference = vertebrae_[0]->pos_ - vertebrae_[1]->pos_;
    newVert->pos_ = vertebrae_[0]->pos_ + difference;
    newVert->dim_ = vertebrae_.front()->dim_;
    //TODO clone the density function
    vertebrae_.insert(vertebrae_.begin(), newVert);

    return newVert;
}

/// Add a vertebrae to the reat
SpinalPiece::Vertebrae* SpinalPiece::AddTailBone()
{
    Vertebrae* newVert = new Vertebrae();
    Vec3 diff = vertebrae_[vertebrae_.size()-1]->pos_ - vertebrae_[vertebrae_.size() - 2]->pos_;
    newVert->pos_ = vertebrae_.back()->pos_ + diff;
    newVert->dim_ = vertebrae_.back()->dim_;

    vertebrae_.push_back(newVert);

    return newVert;
}

/// Remove a vertebrae if allowed
bool SpinalPiece::RemoveBone(Vertebrae* vert)
{
    if (vertebrae_.size() > 2)
    {
        auto found = std::find(vertebrae_.begin(), vertebrae_.end(), vert);
        if (found != vertebrae_.end())
        {
            delete *found;
            vertebrae_.erase(found);
            return true;
        }
    }
    return false;
}

BoundingBox SpinalPiece::ComputeBounds() const
{
    BoundingBox bounds;
    for (auto vert : vertebrae_)
    {
        bounds.Enclose(vert->pos_ - vert->dim_);
        bounds.Enclose(vert->pos_ + vert->dim_);
    }
    return bounds_ = bounds;
}

unsigned SpinalPiece::CalculateStructuralHash() const
{
    return vertebrae_.size() * 32;
}

void SpinalPiece::GetGizmos(std::vector<std::shared_ptr<Gizmo> >& gizmos)
{
    base::GetGizmos(gizmos);
    for (unsigned i = 0; i < vertebrae_.size(); ++i)
        gizmos.push_back(std::shared_ptr<Gizmo>(new SpinalGizmo(this, i)));
}

void SpinalPiece::DrawDebug(IDebugRender* debug, unsigned flags) const
{
    RGBA color = flags & SPRUE_DEBUG_HOVER ? RGBA::Gold : RGBA::Green;
    color.a = 0.75f;
    if (flags & SPRUE_DEBUG_PASSIVE)
        color.a = 0.1f;
    Mat3x4 baseTransform = GetWorldTransform();
    for (unsigned i = 0; i < ((int)vertebrae_.size()) - 1; ++i)
    {
        const Vec3 dir = (vertebrae_[i + 1]->pos_ - vertebrae_[i]->pos_);

        Sphere ring;
        ring.r = vertebrae_[i]->dim_.MaxElement() / 2;

        Vec3 heightVec(0, 0, dir.Length());
        Vec3 offsetXVec(ring.r, 0, 0);
        Vec3 offsetZVec(0, ring.r, 0);
        
        Vec3 fDir(&dir.x);
        fDir.Normalize();

        Quat q = Quat::LookAt(Vec3(0,0,1), fDir, Vec3(0,1,0), Vec3(0,1,0));
        q.Normalize();
        Mat3x4 transform;
        transform.SetTranslatePart(Vec3(&vertebrae_[i]->pos_.x));
        transform.SetRotatePart(q);
        transform = baseTransform * transform;
        //Mat3x4 transform(vertebrae_[i]->pos_, q, Vec3(1,1,1));

        for (float i = 0; i < 360; i += 90.0f)
        {
            Vec3 pt1 = POINT_ON_SPHERE(ring, 90, i);
            Vec3 pt2 = POINT_ON_SPHERE(ring, 90, i + 90.0f);

            Vec3 p1 = (transform * pt1);
            Vec3 p2 = (transform * pt2);
            debug->DrawLine(p1, p2, color);
            //debug->DrawLine(transform * (pt1), transform * (pt2), RGBA::Red);
        }

        Vec3 fHeightVec(0, 0, dir.Length());
        Vec3 fOffsetXVec(ring.r, 0, 0);
        Vec3 fOffsetZVec(0, ring.r, 0);

#define DO_LINE(OFFSET) { math::float3 a = transform * (OFFSET); math::float3 b = transform * fHeightVec; \
            debug->DrawLine(a, b, color); }

        DO_LINE(fOffsetXVec);
        DO_LINE(-fOffsetXVec);
        DO_LINE(fOffsetZVec);
        DO_LINE(-fOffsetZVec);

    }

    for (int i = 0; i < vertebrae_.size(); ++i)
    {
        float radius = vertebrae_[i]->dim_.MaxElement();
        if (i == 0)
        {
            Vec3 dir = (vertebrae_[i + 1]->pos_ - vertebrae_[i]->pos_).Normalized();
            debug->DrawDisc(baseTransform * vertebrae_[i]->pos_, radius, baseTransform.RotatePart() * dir, color);
        }
        else if (i == vertebrae_.size() - 1)
        {
            Vec3 dir = (vertebrae_[i]->pos_ - vertebrae_[i-1]->pos_).Normalized();
            debug->DrawDisc(baseTransform * vertebrae_[i]->pos_, radius, baseTransform.RotatePart() * dir, color);
        }
        else
        {
            Vec3 nextDir = (vertebrae_[i + 1]->pos_ - vertebrae_[i]->pos_).Normalized();
            Vec3 prevDir = (vertebrae_[i]->pos_ - vertebrae_[i-1]->pos_).Normalized();
            debug->DrawDisc(baseTransform * vertebrae_[i]->pos_, radius, baseTransform.RotatePart() * nextDir.Lerp(prevDir, 0.5f).Normalized(), color);
        }
    }
}

VariantVector SpinalPiece::GetPoints() const
{
    VariantVector ret;
    for (auto vert : vertebrae_)
    {
        ret.push_back(vert->pos_);
        ret.push_back(vert->dim_);
    }
    return ret;
}

void SpinalPiece::SetPoints(const VariantVector& pts)
{
    unsigned ptCt = pts.size() / 2;
    if (ptCt !=  vertebrae_.size())
        vertebrae_.resize(ptCt);
    for (unsigned i = 0; i < vertebrae_.size(); ++i)
        if (!vertebrae_[i])
            vertebrae_[i] = new Vertebrae();

    for (unsigned i = 0, vert = 0; i < pts.size() && vert < vertebrae_.size(); i += 2, vert++)
    {
        vertebrae_[vert]->pos_ = pts[i].getVec3();
        vertebrae_[vert]->dim_ = pts[i+1].getVec3();
    }
}

unsigned SpinalPiece::GetNearestVertebrae(const Vec3& point) const
{
    NearestIndex<unsigned> nearest(0);
    for (unsigned i = 1; i < vertebrae_.size(); ++i)
        nearest.Check(LineSegment(vertebrae_[i - 1]->pos_, vertebrae_[i]->pos_).DistanceSq(point), i - 1);
    return nearest.closest_;
}

void SpinalPiece::WriteParameters(Serializer* serializer)
{
    serializer->WriteFloat((float)vertebrae_.size());
    for (unsigned i = 0; i < vertebrae_.size() - 1; ++i)
    {
        serializer->WriteVector3(vertebrae_[i]->pos_);
        serializer->WriteVector3(vertebrae_[i]->dim_);
        serializer->WriteVector3(vertebrae_[i + 1]->pos_);
        serializer->WriteVector3(vertebrae_[i + 1]->dim_);
    }
}

SpinalVertebrae::SpinalVertebrae() :
    base()
{

}

SpinalVertebrae::~SpinalVertebrae()
{

}

void SpinalVertebrae::Register(Context* context)
{
    context->RegisterFactory<SpinalVertebrae>("SpinalVertebrae", "A bone vertex in a Spinal / Caudal chain.");
    context->CopyBaseProperties("SpruePiece", "SpinalVertebrae");
    // Rotation is secret because it will be inferred by the arrangement of vertebrae positions.
    context->GetProperty("SpinalVertebrae", "Rotation")->SetFlags(PS_Secret);
    // Scale is not allowed to be edited.
    context->RemoveProperty("SpinalVertebrae", "Scale");

    REGISTER_PROPERTY_CONST_SET(SpinalVertebrae, Vec3, GetDimensions, SetDimensions, Vec3(), "Dimensions", "Ellipsoid dimensions for controlling the bone's geometric shape.", PS_Default);
    REGISTER_PROPERTY(SpinalVertebrae, bool, IsOutputBone, SetOutputBone, true, "Output Skeleton Bone", "Whether an actual skeleton bone should be output or not", PS_Default);
    REGISTER_PROPERTY(SpinalVertebrae, bool, WasAutoPlaced, SetAutoPlaced, false, "Autoplaced", "Whether this bone was created as part of an automatic insertion/smoothing method.", PS_Secret);
}

BoundingBox SpinalVertebrae::ComputeBounds() const
{
    return bounds_;
}

bool SpinalVertebrae::AcceptAsParent(const SceneObject* possible) const
{
    return dynamic_cast<const SpinalPiece*>(possible) != 0x0;
}

float SpinalVertebrae::GetDensity(const Vec3& pos) const
{
    return 1000.0f;
}

}