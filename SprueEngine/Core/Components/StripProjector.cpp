#include "StripProjector.h"

#include <SprueEngine/Core/Context.h>
#include <SprueEngine/IDebugRender.h>

#include <SprueEngine/MathGeoLib/Geometry/Polygon.h>

namespace SprueEngine
{

    struct StripProjector::StripGizmo : public Gizmo
    {
        unsigned index_;
        Node* vert_;

        StripGizmo(StripProjector* spine, unsigned index) :
            Gizmo(spine, GIZ_Translate | GIZ_Rotate | GIZ_Scale | GIZ_InitialControlPoint),
            index_(index)
        {
            gizmoId_ = index + 1;
            vert_ = spine->nodes_[index];
            transform_ = Mat3x4::FromTRS(spine->GetWorldTransform() * vert_->position_, vert_->rotation_, Vec3(1, 1, 1));
        }

        virtual bool UpdateValue() override
        {
            StripProjector* spine = (StripProjector*)source_;
            Mat3x4 inverse = spine->GetWorldTransform().Inverted();

            vert_->position_ = inverse * transform_.TranslatePart();
            vert_->rotation_ = transform_.RotatePart().ToQuat().Normalized();
            vert_->width_ = vert_->width_ * transform_.GetScale().MaxElement();

            return true;
        }

        virtual void RefreshValue() override
        {
            StripProjector* spine = (StripProjector*)source_;
            transform_ = Mat3x4::FromTRS(spine->GetWorldTransform() * vert_->position_, vert_->rotation_, Vec3(1, 1, 1));
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
                if (StripProjector* spine = (StripProjector*)source_)
                {
                    Node* v = new Node();
                    Node* current = *(spine->nodes_.begin() + index_);
                    Node* next = 0x0;
                    if (index_ < spine->nodes_.size() - 1)
                        next = *(spine->nodes_.begin() + index_ + 1);

                    if (next)
                    {
                        v->position_ = Lerp(current->position_, next->position_, 0.5f);
                        v->rotation_ = current->rotation_.Slerp(next->rotation_, 0.5f);
                        v->width_ = Lerp(current->width_, next->width_, 0.5f);
                    }
                    else
                    {
                        next = *(spine->nodes_.begin() + index_ - 1);
                        if (next)
                        {
                            v->position_ = current->position_ + (current->position_ - next->position_);
                            v->rotation_ = current->rotation_;
                            v->width_ = current->width_;
                        }
                        else
                        {
                            v->position_ = current->position_ + Vec3(1, 1, 1);
                            v->rotation_ = current->rotation_;
                            v->width_ = current->width_;
                        }
                    }

                    if (index_ == spine->nodes_.size() - 1)
                        spine->nodes_.push_back(v);
                    else
                        spine->nodes_.insert(spine->nodes_.begin() + index_ + 1, v);

                    return true;
                }
            }
            else if (commandID == SPRUE_GIZ_CMD_INSERT_BEFORE)
            {
                if (StripProjector* spine = (StripProjector*)source_)
                {
                    Node* v = new Node();
                    Node* current = *(spine->nodes_.begin() + index_);
                    Node* previous = 0x0;
                    if (index_ > 0)
                        previous = *(spine->nodes_.begin() + index_ - 1);

                    if (previous)
                    {
                        v->position_ = Lerp(previous->position_, current->position_, 0.5f);
                        v->rotation_ = previous->rotation_.Slerp(current->rotation_, 0.5f);
                        v->width_ = Lerp(previous->width_, current->width_, 0.5f);
                    }
                    else
                    {
                        delete v;
                        return false;
                    }

                    spine->nodes_.insert(spine->nodes_.begin() + index_, v);

                    return true;
                }
            }
            else if (commandID == SPRUE_GIZ_CMD_REMOVE)
            {
                if (StripProjector* spine = (StripProjector*)source_)
                {
                    if (spine->nodes_.size() <= 2)
                        return false;
                    Node* erased = *(spine->nodes_.begin() + index_);
                    spine->nodes_.erase(spine->nodes_.begin() + index_);
                    delete erased;
                    return true;
                }
            }
            return false;
        }

        virtual bool Equals(Gizmo* rhs) const override
        {
            if (auto other = dynamic_cast<StripGizmo*>(rhs))
                return other->source_ == source_ && other->vert_ == vert_;
            return false;
        }
    };

    StripProjector::StripProjector() :
        TexturingComponent()
    {
        Node* v = new Node();
        v->position_ = Vec3(0, 0, 0);
        nodes_.push_back(v);

        v = new Node();
        v->position_ = Vec3(0, 3, 5);
        nodes_.push_back(v);

        v = new Node();
        v->position_ = Vec3(0, 7, 15);
        nodes_.push_back(v);
    }

    StripProjector::~StripProjector()
    {

    }

    void StripProjector::Register(Context* context)
    {
        context->RegisterFactory<StripProjector>("StripProjector", "");
        context->CopyBaseProperties("TexturingComponent", "StripProjector");
        REGISTER_PROPERTY_CONST_SET(StripProjector, VariantVector, GetPoints, SetPoints, VariantVector(), "Points", "Secret list of points", PS_Secret);
    }

    BoundingBox StripProjector::ComputeBounds() const
    {
        return BoundingBox();
    }

    void StripProjector::GetGizmos(std::vector<std::shared_ptr<Gizmo> >& gizmos)
    {
        base::GetGizmos(gizmos);
        for (unsigned i = 0; i < nodes_.size(); ++i)
            gizmos.push_back(std::shared_ptr<Gizmo>(new StripGizmo(this, i)));
    }

    VariantVector StripProjector::GetPoints() const
    {
        VariantVector ret;
        for (auto vert : nodes_)
        {
            ret.push_back(vert->position_);
            ret.push_back(vert->rotation_);
            ret.push_back(vert->width_);
        }
        return ret;
    }

    void StripProjector::SetPoints(const VariantVector& pts)
    {
        unsigned ptCt = pts.size() / 3;
        if (ptCt != nodes_.size())
            nodes_.resize(ptCt);
        for (unsigned i = 0; i < nodes_.size(); ++i)
            if (!nodes_[i])
                nodes_[i] = new Node();

        for (unsigned i = 0, vert = 0; i < pts.size() && vert < nodes_.size(); i += 3, vert++)
        {
            nodes_[vert]->position_ = pts[i].getVec3();
            nodes_[vert]->rotation_ = pts[i + 1].getQuat();
            nodes_[vert]->width_ = pts[i + 2].getFloat();
        }
    }

    void StripProjector::DrawDebug(IDebugRender* debug, unsigned flags) const
    {
        RGBA color = flags & SPRUE_DEBUG_HOVER ? RGBA::Gold : RGBA::Green;
        color.a = 0.75f;
        if (flags & SPRUE_DEBUG_PASSIVE)
        {
            color = RGBA::Cyan;
            color.a = 0.1f;
        }

        auto trans = GetWorldTransform();
        for (unsigned i = 0; i < nodes_.size() - 1; ++i)
        {
            const auto cur = nodes_[i];
            const auto next = nodes_[i + 1];

            const Vec3 startPos = cur->position_;
            const Vec3 startUp = cur->rotation_ * Vec3::PositiveY;
            const Vec3 startRight = cur->rotation_ * Vec3::PositiveX;
            const Vec3 nextPos = next->position_;
            const Vec3 nextUp = next->rotation_ * Vec3::PositiveY;
            const Vec3 nextRight = next->rotation_ * Vec3::PositiveX;

            debug->DrawLine(trans * (startPos + -startRight * cur->width_), trans * (startPos + startRight * cur->width_), color);
            debug->DrawLine(trans * (nextPos + -nextRight * next->width_), trans * (nextPos + nextRight * next->width_), color);

            debug->DrawLine(trans * (startPos + -startRight * cur->width_), trans * (nextPos + -nextRight * next->width_), color);
            debug->DrawLine(trans * (startPos + startRight * cur->width_), trans * (nextPos + nextRight * next->width_), color);
        }
    }

    RGBA StripProjector::SampleColorProjection(const Vec3& position, const Vec3& normal) const
    {
        const auto inverseTrans = GetWorldTransform().Inverted();
        const Vec3 localPosition = inverseTrans * position;
        const Vec3 localNormal = (inverseTrans.RotatePart() * normal).Normalized();

        auto trans = GetWorldTransform();
        for (unsigned i = 0; i < nodes_.size() - 1; ++i)
        {
            const auto cur = nodes_[i];
            const auto next = nodes_[i + 1];

            const float startWidth = cur->width_;
            const float nextWidth = next->width_;
            const Vec3 startPos = cur->position_;
            const Vec3 startUp = cur->rotation_ * Vec3::PositiveY;
            const Vec3 startRight = cur->rotation_ * Vec3::PositiveX;
            const Vec3 nextPos = next->position_;
            const Vec3 nextUp = next->rotation_ * Vec3::PositiveY;
            const Vec3 nextRight = next->rotation_ * Vec3::PositiveX;

            Polygon poly;
            poly.p.push_back(trans * (startPos - startRight * startWidth).ToPos4());
            poly.p.push_back(trans * (nextPos - nextRight * nextWidth).ToPos4());
            poly.p.push_back(trans * (nextPos + nextRight * nextWidth).ToPos4());
            poly.p.push_back(trans * (startPos + startRight * startWidth).ToPos4());

            auto pt = poly.PlaneCW().ClosestPoint(localPosition.ToPos4());
            if (poly.Contains(pt) && (localPosition - pt).Normalized().Dot(poly.NormalCW()) > 0.0f)
                return RGBA::Red;
        }

        return RGBA::Invalid;
    }
}